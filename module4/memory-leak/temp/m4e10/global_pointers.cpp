#include <windows.h>
#include <stdio.h>
#include <evntprov.h>

static const GUID ProviderGuid = 
{ 0x92345678, 0x1234, 0x1234, { 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF8 } };

REGHANDLE hProvider = 0;

void LogEvent(PCWSTR message)
{
    if (hProvider)
    {
        EVENT_DATA_DESCRIPTOR eventData[1];
        EventDataDescCreate(&eventData[0], message, (ULONG)((wcslen(message) + 1) * sizeof(WCHAR)));
        EVENT_DESCRIPTOR descriptor = { 1, 0, 0, 4, 0, 0, 0 };
        EventWrite(hProvider, &descriptor, 1, eventData);
    }
}

char* globalBuffer = nullptr;
int* globalArray = nullptr;

void InitializeGlobals()
{
    globalBuffer = new char[2048];
    globalArray = new int[1000];
}

void UseGlobals()
{
    if (globalBuffer)
    {
        sprintf_s(globalBuffer, 2048, "Processing data...");
    }
    if (globalArray)
    {
        for (int i = 0; i < 1000; i++)
        {
            globalArray[i] = i;
        }
    }
}

void ReinitializeGlobals()
{
    globalBuffer = new char[2048];
    globalArray = new int[1000];
}

int main()
{
    EventRegister(&ProviderGuid, NULL, NULL, &hProvider);
    LogEvent(L"Processing started");

    InitializeGlobals();
    UseGlobals();

    for (int i = 0; i < 400; i++)
    {
        ReinitializeGlobals();
        UseGlobals();
    }

    LogEvent(L"Processing completed");
    EventUnregister(hProvider);

    printf("\nPress Enter to exit...");
    getchar();
    return 0;
}

