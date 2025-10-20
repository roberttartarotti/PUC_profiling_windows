#include <windows.h>
#include <stdio.h>
#include <evntprov.h>
#include <vector>

static const GUID ProviderGuid = 
{ 0xF2345678, 0x1234, 0x1234, { 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xE6 } };

REGHANDLE hProvider = 0;
HANDLE hEvent = NULL;

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

DWORD WINAPI ShortLivedThread(LPVOID lpParam)
{
    volatile int result = 0;
    
    for (int i = 0; i < 100; i++)
    {
        result += i;
    }
    
    WaitForSingleObject(hEvent, 1);
    
    return 0;
}

void CreateManyShortThreads()
{
    std::vector<HANDLE> threads;
    
    for (int i = 0; i < 1000; i++)
    {
        HANDLE hThread = CreateThread(NULL, 0, ShortLivedThread, NULL, 0, NULL);
        threads.push_back(hThread);
        
        if (i % 100 == 0)
        {
            SetEvent(hEvent);
        }
    }
    
    SetEvent(hEvent);
    
    for (HANDLE hThread : threads)
    {
        WaitForSingleObject(hThread, INFINITE);
        CloseHandle(hThread);
    }
}

int main()
{
    hEvent = CreateEvent(NULL, TRUE, FALSE, NULL);
    EventRegister(&ProviderGuid, NULL, NULL, &hProvider);
    LogEvent(L"Processing started");

    for (int i = 0; i < 50; i++)
    {
        ResetEvent(hEvent);
        CreateManyShortThreads();
    }

    LogEvent(L"Processing completed");
    EventUnregister(hProvider);
    CloseHandle(hEvent);

    printf("\nPress Enter to exit...");
    getchar();
    return 0;
}

