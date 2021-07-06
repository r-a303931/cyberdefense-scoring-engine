#include "pch.h";
#include "WinBase.h";

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

extern "C" UINT32 GetLogon32_Logon_Network() {
    return LOGON32_LOGON_NETWORK;
}

extern "C" UINT32 GetLogon32_Provider_Default() {
    return LOGON32_PROVIDER_DEFAULT;
}
