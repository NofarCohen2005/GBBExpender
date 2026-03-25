#include "Messages/Nofar.h"

class MonitorNofarMessage : public GBBMonitor ::CMessageHandler {
public:
    MonitorNofarMessage(int id) : GBBMonitor::CMessageHandler(NofarMessage) {}
    
    virtual void FillData(char* pData)
    {
        HT::NofarMessage* pMessage = (HT::NofarMessage*)pData;
        AddFieldInt("Field1", pMessage->Field1);
    }
};