#pragma once
#include "Descriptors/MonitorUtilDef.h"
#include "GeneralTypes.h"
#include "AppObjectDefs.h"
#include "Inc/AppObjectDefs.h"

namespace HT {
    struct TestMessage {
        int TestProperty;

        TestMessage()
            : TestProperty(0123)
        {
        }
    };
}
