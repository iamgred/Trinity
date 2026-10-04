#include <Windows.h>
#include <stdio.h>
#include <winternl.h>

#define STATUS_SUCCESS 0x00000000
#define NT_SUCCESS(STATUS) ((NTSTATUS)(STATUS) >= 0)
