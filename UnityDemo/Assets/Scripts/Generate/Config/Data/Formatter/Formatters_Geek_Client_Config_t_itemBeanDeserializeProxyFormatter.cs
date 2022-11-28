// <auto-generated>
// THIS (.cs) FILE IS GENERATED BY MPC(MessagePack-CSharp). DO NOT CHANGE IT.
// </auto-generated>

#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168
#pragma warning disable CS1591 // document public APIs

#pragma warning disable SA1129 // Do not use default value type constructor
#pragma warning disable SA1309 // Field names should not begin with underscore
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
#pragma warning disable SA1403 // File may only contain a single namespace
#pragma warning disable SA1649 // File name should match first type name

namespace Formatters.Geek.Client.Config
{
    public sealed class t_itemBeanDeserializeProxyFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::Geek.Client.Config.t_itemBeanDeserializeProxy>
    {
        // sheetName
        private static global::System.ReadOnlySpan<byte> GetSpan_sheetName() => new byte[1 + 9] { 169, 115, 104, 101, 101, 116, 78, 97, 109, 101 };
        // datas
        private static global::System.ReadOnlySpan<byte> GetSpan_datas() => new byte[1 + 5] { 165, 100, 97, 116, 97, 115 };

        public void Serialize(ref global::MessagePack.MessagePackWriter writer, global::Geek.Client.Config.t_itemBeanDeserializeProxy value, global::MessagePack.MessagePackSerializerOptions options)
        {
            if (value is null)
            {
                writer.WriteNil();
                return;
            }

            var formatterResolver = options.Resolver;
            writer.WriteMapHeader(2);
            writer.WriteRaw(GetSpan_sheetName());
            global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<string>(formatterResolver).Serialize(ref writer, value.sheetName, options);
            writer.WriteRaw(GetSpan_datas());
            global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::Geek.Client.Config.t_itemBeanDeserializeProxyData>(formatterResolver).Serialize(ref writer, value.datas, options);
        }

        public global::Geek.Client.Config.t_itemBeanDeserializeProxy Deserialize(ref global::MessagePack.MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil())
            {
                return null;
            }

            options.Security.DepthStep(ref reader);
            var formatterResolver = options.Resolver;
            var length = reader.ReadMapHeader();
            var ____result = new global::Geek.Client.Config.t_itemBeanDeserializeProxy();

            for (int i = 0; i < length; i++)
            {
                var stringKey = global::MessagePack.Internal.CodeGenHelpers.ReadStringSpan(ref reader);
                switch (stringKey.Length)
                {
                    default:
                    FAIL:
                      reader.Skip();
                      continue;
                    case 9:
                        if (!global::System.MemoryExtensions.SequenceEqual(stringKey, GetSpan_sheetName().Slice(1))) { goto FAIL; }

                        ____result.sheetName = global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<string>(formatterResolver).Deserialize(ref reader, options);
                        continue;
                    case 5:
                        if (global::MessagePack.Internal.AutomataKeyGen.GetKey(ref stringKey) != 495556256100UL) { goto FAIL; }

                        ____result.datas = global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::Geek.Client.Config.t_itemBeanDeserializeProxyData>(formatterResolver).Deserialize(ref reader, options);
                        continue;

                }
            }

            reader.Depth--;
            return ____result;
        }
    }

}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612

#pragma warning restore SA1129 // Do not use default value type constructor
#pragma warning restore SA1309 // Field names should not begin with underscore
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
#pragma warning restore SA1403 // File may only contain a single namespace
#pragma warning restore SA1649 // File name should match first type name
