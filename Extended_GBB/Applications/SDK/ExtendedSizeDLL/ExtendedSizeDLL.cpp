#include "Messages/NewMessage.h"
#include "Descriptors/TestDescriptor.h"
#include "Descriptors/Nofar.h"

#include "Messages/NofarMessage.h"
#include "Messages/RefactoredMessage.h"
#include "Messages/F.h"
#include "Messages/TestMessage.h"

public ExtendedSizeDLL(){
    ADD_DESC1(Nofar);

    ADD_MESSAGE("NofarMessage", NofarMessage, NofarMessage);
    ADD_DESC1(TestDescriptor);
    ADD_MESSAGE("NewMessage", NewMessage, NewMessage);
    ADD_MESSAGE("RefactoredMessage", RefactoredMessage, RefactoredMessage);
    ADD_MESSAGE("F", F, F);
    ADD_MESSAGE("TestMessage", TestMessage, TestMessage);
}
