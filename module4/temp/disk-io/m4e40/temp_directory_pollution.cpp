#include <windows.h>
#include <stdio.h>
#include <evntprov.h>

static const GUID ProviderGuid = 
{ 0xA5345678, 0x1234, 0x1234, { 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF7 } };

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

void CreateTempFiles()
{
    wchar_t tempPath[MAX_PATH];
    GetTempPathW(MAX_PATH, tempPath);
    
    for (int i = 0; i < 1000; i++)
    {
        wchar_t filename[MAX_PATH];
        swprintf_s(filename, MAX_PATH, L"%stempdata_%d_%u.tmp", tempPath, i, GetTickCount());
        
        HANDLE hFile = CreateFileW(filename,
                                   GENERIC_WRITE,
                                   0,
                                   NULL,
                                   CREATE_ALWAYS,
                                   FILE_ATTRIBUTE_TEMPORARY,
                                   NULL);
        
        if (hFile != INVALID_HANDLE_VALUE)
        {
            char data[4096];
            DWORD bytesWritten;
            
            for (int j = 0; j < 4096; j++)
            {
                data[j] = (char)(j % 256);
            }
            
            for (int k = 0; k < 10; k++)
            {
                WriteFile(hFile, data, 4096, &bytesWritten, NULL);
            }
            
            CloseHandle(hFile);
        }
    }
}

int main()
{
    EventRegister(&ProviderGuid, NULL, NULL, &hProvider);
    LogEvent(L"Processing started");

    for (int i = 0; i < 20; i++)
    {
        CreateTempFiles();
    }

    LogEvent(L"Processing completed");
    EventUnregister(hProvider);

    printf("\nPress Enter to exit...");
    getchar();
    return 0;
}

