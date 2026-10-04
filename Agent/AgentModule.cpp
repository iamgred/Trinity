#include "AgentModule.h"
#include <windows.h>
#include <stdio.h>

std::string ConvertLPWSTRToStdString(LPWSTR lpwstr)
{
	if (!lpwstr) return "";	
	int size = WideCharToMultiByte(CP_UTF8, 0, lpwstr, -1, NULL, 0, NULL, NULL);

	if (size <= 0) return "";
	std::string result(size - 1, 0);
	WideCharToMultiByte(CP_UTF8, 0, lpwstr, -1, &result[0], size, NULL, NULL);

	return result;
}

//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
AgentModule::AgentModule()
{
}

//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
BOOL AgentModule::GetUsername(OUT LPWSTR* szWhoamiStr)
{
	DWORD dwSize = 0x00;
	BOOL bResult = FALSE;
	
	if (!GetUserNameW(NULL, &dwSize) && GetLastError() != ERROR_INSUFFICIENT_BUFFER)
	{
		std::cout << "[x] GetUserNameW [%d] Failed With Error: %d \n", __LINE__, GetLastError();
		goto _END_OF_FUNC;
	}

	if (!(*szWhoamiStr = (LPWSTR)LocalAlloc(LPTR, dwSize * sizeof(WCHAR))))
	{
		std::cout << "[x] LocalAlloc Failed With Error: %d \n", GetLastError();
		goto _END_OF_FUNC;
	}

	if (!GetUserNameW(*szWhoamiStr, &dwSize))
	{
		std::cout << "[x] GetUserNameW Failed With Error: %d \n", __LINE__, GetLastError();
		goto _END_OF_FUNC;
	}

	bResult = TRUE;

_END_OF_FUNC:
	if (!bResult && *szWhoamiStr)
	{
		LocalFree(*szWhoamiStr);
	}
	return bResult;
}

//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//

