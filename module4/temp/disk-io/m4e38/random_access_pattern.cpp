#include <windows.h>
#include <stdio.h>
#include <evntprov.h>
#include <stdlib.h>

static const GUID ProviderGuid = 
{ 0xE4345678, 0x1234, 0x1234, { 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF5 } };

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

void RandomAccessWrites()
{
    HANDLE hFile = CreateFileW(L"random_access.dat",
                               GENERIC_WRITE,
                               0,
                               NULL,
                               CREATE_ALWAYS,
                               FILE_ATTRIBUTE_NORMAL,
                               NULL);
    
    if (hFile != INVALID_HANDLE_VALUE)
    {
        char buffer[512];
        DWORD bytesWritten;
        LARGE_INTEGER fileSize;
        fileSize.QuadPart = 100 * 1024 * 1024;
        
        SetFilePointerEx(hFile, fileSize, NULL, FILE_BEGIN);
        SetEndOfFile(hFile);
        
        for (int i = 0; i < 512; i++)
        {
            buffer[i] = (char)(i % 256);
        }
        
        for (int i = 0; i < 50000; i++)
        {
            LARGE_INTEGER offset;
            offset.QuadPart = (rand() % (100 * 1024 * 2)) * 512LL;
            
            SetFilePointerEx(hFile, offset, NULL, FILE_BEGIN);
            WriteFile(hFile, buffer, 512, &bytesWritten, NULL);
        }
        
        CloseHandle(hFile);
    }
    
    DeleteFileW(L"random_access.dat");
}

int main()
{
    EventRegister(&ProviderGuid, NULL, NULL, &hProvider);
    LogEvent(L"Processing started");

    for (int i = 0; i < 10; i++)
    {
        RandomAccessWrites();
    }

    LogEvent(L"Processing completed");
    EventUnregister(hProvider);

    printf("\nPress Enter to exit...");
    getchar();
    return 0;
}

