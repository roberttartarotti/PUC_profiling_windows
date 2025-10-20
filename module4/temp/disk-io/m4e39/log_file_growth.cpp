#include <windows.h>
#include <stdio.h>
#include <evntprov.h>
#include <thread>
#include <chrono>

static const GUID ProviderGuid = 
{ 0xF4345678, 0x1234, 0x1234, { 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF6 } };

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

void WriteLogEntries()
{
    HANDLE hFile = CreateFileW(L"application.log",
                               GENERIC_WRITE,
                               FILE_SHARE_READ,
                               NULL,
                               OPEN_ALWAYS,
                               FILE_ATTRIBUTE_NORMAL,
                               NULL);
    
    if (hFile != INVALID_HANDLE_VALUE)
    {
        SetFilePointer(hFile, 0, NULL, FILE_END);
        
        char logEntry[512];
        DWORD bytesWritten;
        
        for (int i = 0; i < 10000; i++)
        {
            SYSTEMTIME st;
            GetLocalTime(&st);
            
            sprintf_s(logEntry, 512,
                     "%04d-%02d-%02d %02d:%02d:%02d.%03d [INFO] Processing item %d with extensive details and context information that makes each log entry quite large\r\n",
                     st.wYear, st.wMonth, st.wDay, st.wHour, st.wMinute, st.wSecond, st.wMilliseconds, i);
            
            WriteFile(hFile, logEntry, (DWORD)strlen(logEntry), &bytesWritten, NULL);
        }
        
        CloseHandle(hFile);
    }
}

int main()
{
    EventRegister(&ProviderGuid, NULL, NULL, &hProvider);
    LogEvent(L"Processing started");

    for (int i = 0; i < 100; i++)
    {
        WriteLogEntries();
    }

    DeleteFileW(L"application.log");
    
    LogEvent(L"Processing completed");
    EventUnregister(hProvider);

    printf("\nPress Enter to exit...");
    getchar();
    return 0;
}

