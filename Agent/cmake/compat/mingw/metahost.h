// ========================================== { START OF FILE } ========================================== //
// metahost.h — MinGW-w64 shim for the CLR hosting header the Windows SDK ships.
//
// Agent.Legacy/src/Agent.cpp includes <metahost.h> for ICLRMetaHost / ICLRRuntimeInfo and
// CLRCreateInstance. MinGW-w64 ships mscoree.h (ICorRuntimeHost) but not metahost.h, and no
// mscoree import library, so an MSVC build sees the SDK header and a cross build sees this one.
// It lives in cmake/compat/mingw/ and is added to the include path only by the cross toolchain
// (cmake/toolchains/mingw-w64-clang.cmake), so the tree stays byte-identical to upstream and a
// Windows/MSVC build of this directory is unaffected.
//
// Declarations are the SDK's order and values, not invented here: the two interfaces are declared
// in the order metahost.h declares them because a COM vtable is position, and the GUIDs are the
// documented CLSID_CLRMetaHost / IID_ICLRMetaHost / IID_ICLRRuntimeInfo. TeamServer's own agent
// (Agent/src/commands/exec/netasm.cpp) carries the same hand-declared pair and validates the
// corrected slot order against the runtime's type library on every run.
//
// CLRCreateInstance has no import library in MinGW, so it is resolved from mscoree.dll at first
// call exactly as the signature in the SDK header reads. mscoree.dll is part of Windows.
// ========================================== { END OF FILE } ========================================== //

#pragma once

#include <windows.h>
#include <unknwn.h>

// mscoree.h supplies ICorRuntimeHost and the CLSID_/IID_ pair this tree passes to
// ICLRRuntimeInfo::GetInterface. MinGW ships the header but no import library, so the toolchain
// compiles this tree with -DINITGUID: guiddef.h then uses DECLSPEC_SELECTANY definitions, which
// link without libuuid/libmscoree.
#include <mscoree.h>

// The generated include/mscorlib.h declares an interface named `_Nullable` (from mscorlib.tlb).
// Clang treats `_Nullable` as a nullability keyword, so the identifier is a syntax error as-is.
// Renaming it for this translation unit is the smallest change that keeps the generated header
// byte-identical to the upstream copy; only Agent.cpp includes this shim.
#define _Nullable LegacyMscorlibNullable

// ---- GUIDs (values from metahost.h; defined here because MinGW ships neither header nor .a) ----
static const GUID CLSID_CLRMetaHost = {0x9280188D,
                                       0x0E8E,
                                       0x4867,
                                       {0xB3, 0x0C, 0x7F, 0xA8, 0x38, 0x84, 0xE8, 0xDE}};
static const GUID IID_ICLRMetaHost = {0xD332DB9E,
                                      0xB9B3,
                                      0x4125,
                                      {0x82, 0x07, 0xA1, 0x48, 0x84, 0xF5, 0x32, 0x16}};
static const GUID IID_ICLRRuntimeInfo = {0xBD39D1D2,
                                         0xBA2F,
                                         0x486A,
                                         {0x89, 0xB0, 0xB4, 0xB0, 0xCB, 0x46, 0x68, 0x91}};

// ---- ICLRMetaHost: GetRuntime is the first method after IUnknown ----
struct ICLRMetaHost : public IUnknown
{
    virtual HRESULT STDMETHODCALLTYPE GetRuntime(LPCWSTR version, REFIID riid, LPVOID *runtime) = 0;
};

// ---- ICLRRuntimeInfo: declared in SDK order up to the last method this tree calls ----
struct ICLRRuntimeInfo : public IUnknown
{
    virtual HRESULT STDMETHODCALLTYPE GetVersionString(LPWSTR buffer, DWORD *length) = 0;
    virtual HRESULT STDMETHODCALLTYPE GetRuntimeDirectory(LPWSTR buffer, DWORD *length) = 0;
    virtual HRESULT STDMETHODCALLTYPE IsLoaded(HANDLE process, BOOL *loaded) = 0;
    virtual HRESULT STDMETHODCALLTYPE LoadErrorString(UINT resource_id, LPWSTR buffer, DWORD *length,
                                                      LONG locale_id) = 0;
    virtual HRESULT STDMETHODCALLTYPE LoadLibrary(LPCWSTR dll_name, HMODULE *module) = 0;
    virtual HRESULT STDMETHODCALLTYPE GetProcAddress(LPCSTR proc_name, LPVOID *address) = 0;
    virtual HRESULT STDMETHODCALLTYPE GetInterface(REFCLSID clsid, REFIID riid, LPVOID *unknown) = 0;
    virtual HRESULT STDMETHODCALLTYPE IsLoadable(BOOL *loadable) = 0;
};

// ---- __uuidof(mscorlib::_AppDomain) ------------------------------------------------------
// Agent.cpp:252 calls IID_PPV_ARGS(&AppDomain) on an mscorlib::_AppDomain*. MSVC takes the IID
// from the MIDL_INTERFACE attribute; MinGW needs an __uuidof emulation entry, which mscorlib.h
// does not provide. The value is that attribute's, copied from include/mscorlib.h:17954
// (MIDL_INTERFACE("05F696DC-2B29-3663-AD8B-C4389CF2A713") _AppDomain). Declaring the
// specialisation against a forward declaration keeps the generated header untouched.
namespace mscorlib
{
    struct _AppDomain;
}

__CRT_UUID_DECL(mscorlib::_AppDomain, 0x05F696DC, 0x2B29, 0x3663, 0xAD, 0x8B, 0xC4, 0x38, 0x9C, 0xF2, 0xA7, 0x13)

// ---- CLRCreateInstance, resolved from mscoree.dll (no MinGW import library exists) ----
typedef HRESULT(WINAPI *CLRCreateInstanceFn)(REFCLSID clsid, REFIID riid, LPVOID *ppv);

inline HRESULT CLRCreateInstance(REFCLSID clsid, REFIID riid, LPVOID *ppv)
{
    static CLRCreateInstanceFn create_instance = []() -> CLRCreateInstanceFn {
        HMODULE const mscoree = LoadLibraryW(L"mscoree.dll");
        if (mscoree == nullptr)
        {
            return nullptr;
        }
        return reinterpret_cast<CLRCreateInstanceFn>(GetProcAddress(mscoree, "CLRCreateInstance"));
    }();

    if (create_instance == nullptr)
    {
        return static_cast<HRESULT>(0x80004005L); // E_FAIL
    }
    return create_instance(clsid, riid, ppv);
}
