int INTTCMaxDescriptor = 1;

enum  ExtendedGBBdescriptorName{
    DescriptorName = INTTCMaxDescriptor + 1,
    TestDescriptor = INTTCMaxDescriptor + 1,
}
    
enum  ExtendedGBBmessageName{
    DescriptorName = INTTCMaxDescriptor + 1,

    ///<summary>Descriptor for RefactoredMessage</summary>
    RefactoredMessage = INTTCMaxMessage + 1,
    ///<summary>Descriptor for F</summary>
    F = INTTCMaxMessage + 2,
    ///<summary>Descriptor for TestMessage</summary>
    TestMessage = INTTCMaxMessage + 3,
    ExtendedMaxDescriptors = INTTCMaxDescriptor + 1000
    NewMessage = INTTCMaxMessage + 1,
}
