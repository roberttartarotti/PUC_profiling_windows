#include <windows.h>
#include <stdio.h>
#include <evntprov.h>
#include <vector>

static const GUID ProviderGuid = 
{ 0xB2345678, 0x1234, 0x1234, { 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xE2 } };

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

DWORD WINAPI WorkerThread(LPVOID lpParam)
{
    int* data = (int*)lpParam;
    volatile int result = 0;
    
    for (int i = 0; i < 50000; i++)
    {
        result += (*data) * i;
    }
    
    return 0;
}

void ProcessData(int value)
{
    std::vector<HANDLE> threads;
    
    for (int i = 0; i < 500; i++)
    {
        int* data = new int(value + i);
        HANDLE hThread = CreateThread(NULL, 0, WorkerThread, data, 0, NULL);
        threads.push_back(hThread);
    }
    
    for (HANDLE hThread : threads)
    {
        WaitForSingleObject(hThread, INFINITE);
        CloseHandle(hThread);
    }
}

int main()
{
    EventRegister(&ProviderGuid, NULL, NULL, &hProvider);
    LogEvent(L"Processing started");

    for (int i = 0; i < 50; i++)
    {
        ProcessData(i);
    }

    LogEvent(L"Processing completed");
    EventUnregister(hProvider);

    printf("\nPress Enter to exit...");
    getchar();
    return 0;
}

