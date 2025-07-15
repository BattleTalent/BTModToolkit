using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityMcpBridge.Editor.Helpers;

namespace UnityMcpBridge.Editor.Tools
{
    /// <summary>
    /// Handles CRUD operations for Lua scripts within the Unity project.
    /// Updated for MCP integration.
    /// </summary>
    public static class ManageLuaScript
    {
        /// <summary>
        /// Main handler for Lua script management actions.
        /// </summary>
        public static object HandleCommand(JObject @params)
        {
            // Extract parameters
            string action = @params["action"]?.ToString().ToLower();
            string name = @params["name"]?.ToString();
            string path = @params["path"]?.ToString(); // Relative to Assets/
            string contents = null;

            // Check if we have base64 encoded contents
            bool contentsEncoded = @params["contentsEncoded"]?.ToObject<bool>() ?? false;
            if (contentsEncoded && @params["encodedContents"] != null)
            {
                try
                {
                    contents = DecodeBase64(@params["encodedContents"].ToString());
                }
                catch (Exception e)
                {
                    return Response.Error($"Failed to decode Lua script contents: {e.Message}");
                }
            }
            else
            {
                contents = @params["contents"]?.ToString();
            }

            string scriptType = @params["scriptType"]?.ToString(); // For templates/validation
            string className = @params["className"]?.ToString(); // For Lua class name

            // Validate required parameters
            if (string.IsNullOrEmpty(action))
            {
                return Response.Error("Action parameter is required.");
            }
            if (string.IsNullOrEmpty(name))
            {
                return Response.Error("Name parameter is required.");
            }
            // Basic name validation for Lua scripts
            if (!Regex.IsMatch(name, @"^[a-zA-Z_][a-zA-Z0-9_]*$"))
            {
                return Response.Error(
                    $"Invalid Lua script name: '{name}'. Use only letters, numbers, underscores, and don't start with a number."
                );
            }

            // Ensure path is relative to Assets/, removing any leading "Assets/"
            // Set default directory to "Scripts" if path is not provided
            string relativeDir = path ?? "Scripts"; // Default to "Scripts" if path is null
            if (!string.IsNullOrEmpty(relativeDir))
            {
                relativeDir = relativeDir.Replace('\\', '/').Trim('/');
                if (relativeDir.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
                {
                    relativeDir = relativeDir.Substring("Assets/".Length).TrimStart('/');
                }
            }
            // Handle empty string case explicitly after processing
            if (string.IsNullOrEmpty(relativeDir))
            {
                relativeDir = "Scripts"; // Ensure default if path was provided as "" or only "/" or "Assets/"
            }

            // Construct paths - Lua scripts use .txt extension in this project
            string scriptFileName = $"{name}.txt";
            string fullPathDir = Path.Combine(Application.dataPath, relativeDir); // Application.dataPath ends in "Assets"
            string fullPath = Path.Combine(fullPathDir, scriptFileName);
            string relativePath = Path.Combine("Assets", relativeDir, scriptFileName)
                .Replace('\\', '/'); // Ensure "Assets/" prefix and forward slashes

            // Ensure the target directory exists for create/update
            if (action == "create" || action == "update")
            {
                try
                {
                    Directory.CreateDirectory(fullPathDir);
                }
                catch (Exception e)
                {
                    return Response.Error(
                        $"Could not create directory '{fullPathDir}': {e.Message}"
                    );
                }
            }

            // Route to specific action handlers
            switch (action)
            {
                case "create":
                    return CreateLuaScript(
                        fullPath,
                        relativePath,
                        name,
                        contents,
                        scriptType,
                        className
                    );
                case "read":
                    return ReadLuaScript(fullPath, relativePath);
                case "update":
                    return UpdateLuaScript(fullPath, relativePath, name, contents);
                case "delete":
                    return DeleteLuaScript(fullPath, relativePath);
                default:
                    return Response.Error(
                        $"Unknown action: '{action}'. Valid actions are: create, read, update, delete."
                    );
            }
        }

        /// <summary>
        /// Decode base64 string to normal text
        /// </summary>
        private static string DecodeBase64(string encoded)
        {
            byte[] data = Convert.FromBase64String(encoded);
            return System.Text.Encoding.UTF8.GetString(data);
        }

        /// <summary>
        /// Encode text to base64 string
        /// </summary>
        private static string EncodeBase64(string text)
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes(text);
            return Convert.ToBase64String(data);
        }

        private static object CreateLuaScript(
            string fullPath,
            string relativePath,
            string name,
            string contents,
            string scriptType,
            string className
        )
        {
            // Check if script already exists
            if (File.Exists(fullPath))
            {
                return Response.Error(
                    $"Lua script already exists at '{relativePath}'. Use 'update' action to modify."
                );
            }

            // Generate default content if none provided
            if (string.IsNullOrEmpty(contents))
            {
                contents = GenerateDefaultLuaScriptContent(name, scriptType, className);
            }

            // Validate syntax (basic check)
            if (!ValidateLuaScriptSyntax(contents))
            {
                // Optionally return a specific error or warning about syntax
                Debug.LogWarning($"Potential syntax error in Lua script being created: {name}");
            }

            try
            {
                File.WriteAllText(fullPath, contents);
                AssetDatabase.ImportAsset(relativePath);
                AssetDatabase.Refresh(); // Ensure Unity recognizes the new script
                return Response.Success(
                    $"Lua script '{name}.txt' created successfully at '{relativePath}'.",
                    new { path = relativePath }
                );
            }
            catch (Exception e)
            {
                return Response.Error($"Failed to create Lua script '{relativePath}': {e.Message}");
            }
        }

        private static object ReadLuaScript(string fullPath, string relativePath)
        {
            if (!File.Exists(fullPath))
            {
                return Response.Error($"Lua script not found at '{relativePath}'.");
            }

            try
            {
                string contents = File.ReadAllText(fullPath);

                // Return both normal and encoded contents for larger files
                bool isLarge = contents.Length > 10000; // If content is large, include encoded version
                var responseData = new
                {
                    path = relativePath,
                    contents = contents,
                    // For large files, also include base64-encoded version
                    encodedContents = isLarge ? EncodeBase64(contents) : null,
                    contentsEncoded = isLarge,
                };

                return Response.Success(
                    $"Lua script '{Path.GetFileName(relativePath)}' read successfully.",
                    responseData
                );
            }
            catch (Exception e)
            {
                return Response.Error($"Failed to read Lua script '{relativePath}': {e.Message}");
            }
        }

        private static object UpdateLuaScript(
            string fullPath,
            string relativePath,
            string name,
            string contents
        )
        {
            if (!File.Exists(fullPath))
            {
                return Response.Error(
                    $"Lua script not found at '{relativePath}'. Use 'create' action to add a new script."
                );
            }
            if (string.IsNullOrEmpty(contents))
            {
                return Response.Error("Content is required for the 'update' action.");
            }

            // Validate syntax (basic check)
            if (!ValidateLuaScriptSyntax(contents))
            {
                Debug.LogWarning($"Potential syntax error in Lua script being updated: {name}");
                // Consider if this should be a hard error or just a warning
            }

            try
            {
                File.WriteAllText(fullPath, contents);
                AssetDatabase.ImportAsset(relativePath); // Re-import to reflect changes
                AssetDatabase.Refresh();
                return Response.Success(
                    $"Lua script '{name}.txt' updated successfully at '{relativePath}'.",
                    new { path = relativePath }
                );
            }
            catch (Exception e)
            {
                return Response.Error($"Failed to update Lua script '{relativePath}': {e.Message}");
            }
        }

        private static object DeleteLuaScript(string fullPath, string relativePath)
        {
            if (!File.Exists(fullPath))
            {
                return Response.Error($"Lua script not found at '{relativePath}'. Cannot delete.");
            }

            try
            {
                // Use AssetDatabase.MoveAssetToTrash for safer deletion (allows undo)
                bool deleted = AssetDatabase.MoveAssetToTrash(relativePath);
                if (deleted)
                {
                    AssetDatabase.Refresh();
                    return Response.Success(
                        $"Lua script '{Path.GetFileName(relativePath)}' moved to trash successfully."
                    );
                }
                else
                {
                    // Fallback or error if MoveAssetToTrash fails
                    return Response.Error(
                        $"Failed to move Lua script '{relativePath}' to trash. It might be locked or in use."
                    );
                }
            }
            catch (Exception e)
            {
                return Response.Error($"Error deleting Lua script '{relativePath}': {e.Message}");
            }
        }

        /// <summary>
        /// Generates basic Lua script content based on name and type.
        /// </summary>
        private static string GenerateDefaultLuaScriptContent(
            string name,
            string scriptType,
            string className
        )
        {
            string actualClassName = !string.IsNullOrEmpty(className) ? className : name;
            
            string template = "";

            switch (scriptType?.ToLower())
            {
                case "scabbard":
                    template = GenerateScabbardTemplate(actualClassName);
                    break;
                case "weapon":
                    template = GenerateWeaponTemplate(actualClassName);
                    break;
                case "interact":
                    template = GenerateInteractTemplate(actualClassName);
                    break;
                case "character":
                    template = GenerateCharacterTemplate(actualClassName);
                    break;
                case "item":
                    template = GenerateItemTemplate(actualClassName);
                    break;
                default:
                    template = GenerateBasicTemplate(actualClassName);
                    break;
            }

            return template;
        }

        private static string GenerateBasicTemplate(string className)
        {
            return $@"local {className} = {{
    -- Properties
    
}}

function {className}:Awake()
    -- Called when the object is created
    
end

function {className}:Start()
    -- Called before the first frame update
    
end

function {className}:Update()
    -- Called once per frame
    
end

function {className}:OnDestroy()
    -- Called when the object is destroyed
    
end

return Class(nil, nil, {className})
";
        }

        private static string GenerateScabbardTemplate(string className)
        {
            return $@"local {className} = {{
    maxConnectDis = 0.1,
    
    pForce = 1000,
    eForce = 200,
    maxScrollForce = 10000,
    scrollForceOnSlot = 15000,
    breakForce = 10000,
    scrollForce = 1000,
    dragLength = 0.05,
    
    maxAngle = 70,
    jointZOffset = 0.2,
    jointXOffset = 0.4,
    
    isConnect = false,
    isLock = false,
    isEnter = false,
    isIn = false,
    
    swordType = """",
    
    isTwoSide = true,
}}

function {className}:Awake()
    self.col = self.host.gameObject:GetComponent(""Collider"")
    self.rb = self.host.gameObject:GetComponent(""Rigidbody"")
    
    -- Setup event handlers
    self.scabbardGrabCb = function(a, g)
        self:ScabbardOnGrab(a, g)
    end
    self.ib:GrabEvent(""+"", self.scabbardGrabCb)
    
end

function {className}:OnTriggerStay(other)
    -- Handle sword detection and connection
    
end

function {className}:ConnectUpdate()
    -- Update connection logic
    
end

function {className}:BuildConnect(rb)
    -- Build joint connections
    
end

function {className}:Disconnect()
    -- Clean up connections
    
end

function {className}:ScabbardOnGrab(a, g)
    -- Handle scabbard grab events
    
end

function {className}:OnDestroy()
    self.ib:GrabEvent(""-"", self.scabbardGrabCb)
    
end

return Class(nil, nil, {className})
";
        }

        private static string GenerateWeaponTemplate(string className)
        {
            return $@"local {className} = {{
    -- Weapon properties
    damage = 10,
    range = 1.0,
    attackSpeed = 1.0,
    
    isScabbardUser = true,
    swordType = """",
    
    -- States
    isGrabbed = false,
    isSheathed = false,
}}

function {className}:Awake()
    -- Setup weapon components
    self.weaponGrabCb = function(a, g)
        self:OnGrab(a, g)
    end
    self.ib:GrabEvent(""+"", self.weaponGrabCb)
    
end

function {className}:OnGrab(a, g)
    self.isGrabbed = g
    if g then
        -- Weapon grabbed
        
    else
        -- Weapon released
        
    end
end

function {className}:OnSheath(scabbard, isSheathed, time)
    self.isSheathed = isSheathed
    if isSheathed then
        -- Weapon sheathed
        
    else
        -- Weapon unsheathed
        
    end
end

function {className}:GetSwordType()
    return self.swordType
end

function {className}:OnDestroy()
    self.ib:GrabEvent(""-"", self.weaponGrabCb)
    
end

return Class(nil, nil, {className})
";
        }

        private static string GenerateInteractTemplate(string className)
        {
            return $@"local {className} = {{
    -- Interaction properties
    
}}

function {className}:Awake()
    -- Setup interaction handlers
    
end

function {className}:OnTriggerEnter(other)
    -- Handle trigger enter
    
end

function {className}:OnTriggerExit(other)
    -- Handle trigger exit
    
end

function {className}:OnCollisionEnter(collision)
    -- Handle collision enter
    
end

function {className}:OnActivateBegin()
    -- Called when interaction begins
    
end

function {className}:OnActivateEnd()
    -- Called when interaction ends
    
end

function {className}:OnDestroy()
    -- Cleanup
    
end

return Class(nil, nil, {className})
";
        }

        private static string GenerateCharacterTemplate(string className)
        {
            return $@"local {className} = {{
    -- Character properties
    health = 100,
    speed = 5.0,
    
    -- States
    isAlive = true,
    isMoving = false,
}}

function {className}:Awake()
    -- Setup character components
    
end

function {className}:Start()
    -- Initialize character
    
end

function {className}:Update()
    -- Update character logic
    
end

function {className}:TakeDamage(damage)
    self.health = self.health - damage
    if self.health <= 0 then
        self:Die()
    end
end

function {className}:Die()
    self.isAlive = false
    -- Handle death
    
end

function {className}:OnDestroy()
    -- Cleanup
    
end

return Class(nil, nil, {className})
";
        }

        private static string GenerateItemTemplate(string className)
        {
            return $@"local {className} = {{
    -- Item properties
    itemName = """",
    value = 0,
    stackable = false,
    maxStack = 1,
}}

function {className}:Awake()
    -- Setup item
    
end

function {className}:Use()
    -- Use item logic
    
end

function {className}:OnPickup(character)
    -- Handle pickup
    
end

function {className}:OnDrop(character)
    -- Handle drop
    
end

function {className}:OnDestroy()
    -- Cleanup
    
end

return Class(nil, nil, {className})
";
        }

        /// <summary>
        /// Performs a very basic Lua syntax validation.
        /// </summary>
        private static bool ValidateLuaScriptSyntax(string contents)
        {
            if (string.IsNullOrEmpty(contents))
                return true;

            // Basic checks for Lua syntax
            int functionCount = 0;
            int endCount = 0;
            
            // Count function/end pairs
            var lines = contents.Split('\n');
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("function "))
                    functionCount++;
                if (trimmed == "end" || trimmed.StartsWith("end "))
                    endCount++;
            }

            // Check if script ends with return Class pattern
            bool hasReturnClass = contents.TrimEnd().EndsWith(")") && 
                                  contents.Contains("return Class(");

            // Basic validation - function/end balance and return Class pattern
            return functionCount <= endCount && hasReturnClass;
        }
    }
}
// Force Unity recompile
