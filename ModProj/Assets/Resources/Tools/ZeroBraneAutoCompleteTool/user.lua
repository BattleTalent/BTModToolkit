--[[--
  Use this file to specify **User** preferences.
  Review [examples](+C:\Program Files\ZeroBrane\cfg\user-sample.lua) or check [online documentation](http://studio.zerobrane.com/documentation.html) for details.
--]]--
local G = ...
local luaspec = G.ide.specs.lua
luaspec.exts[#luaspec.exts+1] = "txt"
editor.tabwidth = 4
--[[
editor.keymap[#editor.keymap+1] =
  {('Z'):byte(), wxstc.wxSTC_SCMOD_CTRL + wxstc.wxSTC_SCMOD_SHIFT, wxstc.wxSCI_REDO}
  ]]
  
table.insert(api, 'syntax')