using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using TcBuildGenerator;


namespace TcBuild {
    internal class AssemblyParser : IDisposable {
        private readonly ILogger _log;
        private readonly FileInfo _assembly;
        private readonly PEReader _peReader;
        private readonly MetadataReader _mdReader;
        private readonly ICustomAttributeTypeProvider<KnownType> _typeResolver = new TypeResolver();


        internal AssemblyParser(FileInfo assembly, ILogger log)
        {
            _assembly = assembly;
            _log = log;
            _peReader = new PEReader(File.OpenRead(assembly.FullName));
            _mdReader = _peReader.GetMetadataReader(MetadataReaderOptions.None);
        }


        internal PluginDefinition? GetPluginDefinition()
        {
            var definitions = GetPluginDefinitions().ToList();

            if (definitions.Count == 1) {
                return definitions.Single();
            }

            else {
                if (definitions.Count == 0) {
                    _log.LogMessage($"No Plugin found in: '{_assembly.FullName}'");
                }
                else {
                    _log.LogError($"To many plugins found in: '{_assembly.FullName}'. Found {definitions.Count} plugin implementations: '{string.Join(", ", definitions.Select(_ => $"{_.Type}: {_.ClassFullName}"))}'");
                }

                return null;
            }
        }


        internal IEnumerable<PluginDefinition> GetPluginDefinitions()
        {
            foreach (var attrHandle in _mdReader.CustomAttributes) {
                var attribute = _mdReader.GetCustomAttribute(attrHandle);

                if (IsAttributeType(_mdReader, attribute, TcInfos.TcPluginDefinitionAttribute)) {
                    var value = attribute.DecodeValue(_typeResolver);

                    var definition = value switch {
                        { FixedArguments: { Length: 2 } args } when (
                            args[0].Type == KnownType.String &&
                            args[1].Type == KnownType.SystemType
                        ) => new PluginDefinition {
                            Type = (PluginType) Enum.Parse(typeof(PluginType), $"{args[0].Value}"),
                            ClassFullName = $"{args[1].Value}",
                        },
                        _ => null
                    };

                    if (definition == null) {
                        _log.LogWarning($"Found invalid [{TcInfos.TcPluginDefinitionAttribute}] on '{_assembly.Name}'. Parameters: ({string.Join(", ", value.FixedArguments.Select(_ => $"{_.Type} '{_.Value}'"))})");
                        continue;
                    }

                    yield return definition;
                }
            }
        }


        private static bool IsAttributeType(MetadataReader reader, CustomAttribute attribute, string attributeTypeName)
        {
            StringHandle name;
            switch (attribute.Constructor.Kind) {
                case HandleKind.MemberReference:
                    var refConstructor = reader.GetMemberReference((MemberReferenceHandle) attribute.Constructor);
                    var refType = reader.GetTypeReference((TypeReferenceHandle) refConstructor.Parent);
                    name = refType.Name;
                    break;

                case HandleKind.MethodDefinition:
                    var defConstructor = reader.GetMethodDefinition((MethodDefinitionHandle) attribute.Constructor);
                    var defType = reader.GetTypeDefinition(defConstructor.GetDeclaringType());
                    name = defType.Name;
                    break;

                default:
                    Debug.Assert(false, "Unknown attribute constructor kind");
                    return false;
            }

            return reader.StringComparer.Equals(name, attributeTypeName);
        }


        private readonly struct KnownType {
            public static readonly KnownType Unknown = new KnownType("Unknown");
            public static readonly KnownType String = new KnownType("String");
            public static readonly KnownType SystemTypeArray = new KnownType("SystemTypeArray");
            public static readonly KnownType SystemType = new KnownType("SystemType");

            private readonly string _value;
            public KnownType(string value) => _value = value;
            public static implicit operator string(KnownType type) => type._value;
            public override string ToString() => _value;
        }


        private class TypeResolver : ICustomAttributeTypeProvider<KnownType> {
            public KnownType GetPrimitiveType(PrimitiveTypeCode typeCode)
            {
                return typeCode switch {
                    PrimitiveTypeCode.String => KnownType.String,
                    _ => KnownType.Unknown
                };
            }

            public KnownType GetSystemType()
            {
                return KnownType.SystemType;
            }

            public KnownType GetSZArrayType(KnownType elementType)
            {
                if (elementType == KnownType.SystemType) {
                    return KnownType.SystemTypeArray;
                }

                throw new BadImageFormatException("Unexpectedly got an array of unsupported type.");
            }

            public KnownType GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind)
            {
                var type = reader.GetTypeDefinition(handle);

                if (reader.StringComparer.Equals(type.Name, nameof(Type))) {
                    return KnownType.SystemType;
                }

                return KnownType.Unknown;
            }

            public KnownType GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind)
            {
                var type = reader.GetTypeReference(handle);

                if (reader.StringComparer.Equals(type.Name, nameof(Type))) {
                    return KnownType.SystemType;
                }

                return KnownType.Unknown;
            }

            public KnownType GetTypeFromSerializedName(string name)
            {
                return new KnownType(name);
            }

            public PrimitiveTypeCode GetUnderlyingEnumType(KnownType type)
            {
                throw new BadImageFormatException("Unexpectedly got an enum parameter for an attribute.");
            }

            public bool IsSystemType(KnownType type)
            {
                return type == KnownType.SystemType;
            }
        }


        public void Dispose()
        {
            _peReader.Dispose();
        }
    }
}
