#include <windows.h>
#include <stdio.h>
#include <process.h>
#include <evntprov.h>

static const GUID ProviderGuid = 
{ 0xC2345678, 0x1234, 0x1234, { 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xFB } };

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

unsigned __stdcall ThreadFunction(void* param)
{
    int* threadData = new int[1000];
    
    for (int i = 0; i < 1000; i++)
    {
        threadData[i] = i * 2;
    }
    
    return 0;
}

int main()
{
    EventRegister(&ProviderGuid, NULL, NULL, &hProvider);
    LogEvent(L"Processing started");

    HANDLE threads[200];
    for (int i = 0; i < 200; i++)
    {
        threads[i] = (HANDLE)_beginthreadex(NULL, 0, ThreadFunction, NULL, 0, NULL);
    }

    WaitForMultipleObjects(200, threads, TRUE, INFINITE);
    
    for (int i = 0; i < 200; i++)
    {
        CloseHandle(threads[i]);
    }

    LogEvent(L"Processing completed");
    EventUnregister(hProvider);

    printf("\nPress Enter to exit...");
    getchar();
    return 0;
}

