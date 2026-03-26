#include "Descriptors/TestDescriptor.h"
#include "Descriptors/Nofar.h"

class MonitorNofar : public GBBMonitor ::CDescHandler {
public:
    MonitorNofar(int id) : GBBMonitor::CDescHandler(Nofar) {}
    
    virtual void FillData(char* pData)
    {
        HT::Nofar* pDesc = (HT::Nofar*)pData;
        AddFieldInt("Field1", pDesc->Field1);
    }
};
class MonitorTestDescriptor : public GBBMonitor::CDescHandler {
public:
    MonitorTestDescriptor(int id) : GBBMonitor::CDescHandler(TestDescriptor) {}

    virtual void FillData(char* pData)
    {
        HT::TestDescriptor* pDesc = (HT::TestDescriptor*)pData;
        AddFieldInt("FieldA", pDesc->FieldA);
    }
};
