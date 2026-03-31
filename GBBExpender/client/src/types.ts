export type EntryType = 'Descriptor' | 'Message';

export interface Property {
  name: string;
  dataType: string;
  defaultValue: string;
  size?: number;
}

export interface GeneratorRequest {
  entryType: EntryType;
  objectName: string;
  properties: Property[];
}

export const DATA_TYPES = ['int', 'uint', 'double', 'bool', 'short', 'string'];
