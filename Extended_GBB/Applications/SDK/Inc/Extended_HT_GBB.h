int INTTCMaxDescriptor = 1;
int INTTCMaxMessage = 1;


enum  ExtendedGBBdescriptorName{
    Nofar = INTTCMaxDescriptor + 1,
    TestDescriptor = INTTCMaxDescriptor + 1,
};
    
enum  ExtendedGBBmessageName{
    NofarMessage = INTTCMaxMessage + 1,

    RefactoredMessage = INTTCMaxMessage + 2,
    F = INTTCMaxMessage + 3,
    ExtendedMaxMessages = INTTCMaxMessage + 1000
    NewMessage = INTTCMaxMessage + 1,
};
