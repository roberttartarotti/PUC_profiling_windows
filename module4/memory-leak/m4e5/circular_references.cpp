#include <windows.h>
#include <stdio.h>
#include <evntprov.h>
#include <thread>
#include <chrono>

static const GUID ProviderGuid = 
{ 0x42345678, 0x1234, 0x1234, { 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF3 } };

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

class Node
{
public:
    int data[1000];
    Node* next;
    Node* prev;
    
    Node(int value)
    {
        for (int i = 0; i < 1000; i++)
        {
            data[i] = value + i;
        }
        next = nullptr;
        prev = nullptr;
    }
};

void CreateCircularList()
{
    Node* head = new Node(1);
    Node* second = new Node(2);
    Node* third = new Node(3);
    Node* fourth = new Node(4);
    Node* fifth = new Node(5);
    
    head->next = second;
    second->prev = head;
    second->next = third;
    third->prev = second;
    third->next = fourth;
    fourth->prev = third;
    fourth->next = fifth;
    fifth->prev = fourth;
    fifth->next = head;
    head->prev = fifth;
    
    volatile int sum = 0;
    Node* current = head;
    for (int i = 0; i < 5; i++)
    {
        for (int j = 0; j < 1000; j++)
        {
            sum += current->data[j];
        }
        current = current->next;
    }
}

int main()
{
    EventRegister(&ProviderGuid, NULL, NULL, &hProvider);
    LogEvent(L"Processing started");

    for (int i = 0; i < 5000; i++)
    {
        CreateCircularList();
        
        if (i % 100 == 0)
        {
            std::this_thread::sleep_for(std::chrono::milliseconds(25));
        }
    }

    LogEvent(L"Processing completed");
    EventUnregister(hProvider);

    printf("\nPress Enter to exit...");
    getchar();
    return 0;
}

