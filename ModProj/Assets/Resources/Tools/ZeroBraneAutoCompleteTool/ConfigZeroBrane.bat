if defined ZBS (
    echo %ZBS%
    xcopy /s /Y luabase.lua "%ZBS%\interpreters"
    xcopy /s /Y syntax.lua "%ZBS%\api\lua"
    echo d | xcopy /s /Y user.lua "%USERPROFILE%\.zbstudio"    
) else (
	echo no ZBS Env Var Defined
)