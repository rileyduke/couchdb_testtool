﻿/*
* Licensed to the Apache Software Foundation (ASF) under one
* or more contributor license agreements. See the NOTICE file
* distributed with this work for additional information
* regarding copyright ownership. The ASF licenses this file
* to you under the Apache License, Version 2.0 (the
* "License"); you may not use this file except in compliance
* with the License. You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing,
* software distributed under the License is distributed on an
* "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
* Kind, either express or implied. See the License for the
* specific language governing permissions and limitations
* under the License.
*/

using PortCMIS.Data.Extensions;
using PortCMIS.Enums;
using PortCMIS.Exceptions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Text;
using System.Xml;

namespace PortCMIS.Binding.AtomPub
{
    internal abstract class XmlWalker<T>
    {
        public T Walk(XmlReader parser)
        {
            T result = PrepareTarget(parser, parser.LocalName, parser.NamespaceURI);

            if (!parser.IsEmptyElement)
            {
                XmlUtils.Next(parser);

                // walk through all tags
                while (true)
                {
                    XmlNodeType nodeType = parser.NodeType;
                    if (nodeType == XmlNodeType.Element)
                    {
                        if (!Read(parser, parser.LocalName, parser.NamespaceURI, result))
                        {
                            if (result is IExtensionsData)
                            {
                                HandleExtension(parser, (IExtensionsData)result);
                            }
                            else
                            {
                                XmlUtils.Skip(parser);
                            }
                        }
                    }
                    else if (nodeType == XmlNodeType.EndElement)
                    {
                        break;
                    }
                    else
                    {
                        if (!XmlUtils.Next(parser))
                        {
                            break;
                        }
                    }
                }
            }

            XmlUtils.Next(parser);

            return result;
        }

        protected bool IsCmisNamespace(string ns)
        {
            return XmlConstants.NamespaceCmis == ns;
        }

        protected bool IsAtomNamespace(string ns)
        {
            return XmlConstants.NamespaceAtom == ns;
        }

        protected bool IsTag(string name, string tag)
        {
            return name == tag;
        }

        protected void HandleExtension(XmlReader parser, IExtensionsData extData)
        {
            IList<ICmisExtensionElement> extensions = extData.Extensions;
            if (extensions == null)
            {
                extensions = new List<ICmisExtensionElement>();
                extData.Extensions = extensions;
            }

            if (extensions.Count + 1 > XmlConstraints.MaxExtensionsWidth)
            {
                throw new CmisInvalidArgumentException("Too many extensions! (More than " + XmlConstraints.MaxExtensionsWidth + " extensions.)");
            }

            extensions.Add(HandleExtensionLevel(parser, 0));
        }

        private ICmisExtensionElement HandleExtensionLevel(XmlReader parser, int level)
        {
            string localname = parser.Name;
            string ns = parser.NamespaceURI;
            IDictionary<string, string> attributes = null;
            StringBuilder sb = new StringBuilder();
            IList<ICmisExtensionElement> children = null;

            if (parser.HasAttributes)
            {
                attributes = new Dictionary<string, string>();

                while (parser.MoveToNextAttribute())
                {
                    attributes.Add(parser.Name, parser.Value);
                }

                parser.MoveToElement();
            }

            if (!parser.IsEmptyElement)
            {
                XmlUtils.Next(parser);

                while (true)
                {
                    XmlNodeType nodeType = parser.NodeType;
                    if (nodeType == XmlNodeType.EndElement)
                    {
                        break;
                    }
                    else if (nodeType == XmlNodeType.Text || nodeType == XmlNodeType.CDATA)
                    {
                        char[] buffer = new char[8 * 1024];

                        int len;
                        while ((len = parser.ReadValueChunk(buffer, 0, buffer.Length)) > 0)
                        {
                            if (sb.Length + len > XmlConstraints.MaxStringLength)
                            {
                                throw new CmisInvalidArgumentException("String limit exceeded! (String is longer than " + XmlConstraints.MaxStringLength + " characters.)");
                            }

                            sb.Append(buffer, 0, len);
                        }
                    }
                    else if (nodeType == XmlNodeType.Element)
                    {
                        if (level + 1 > XmlConstraints.MaxExtensionsDepth)
                        {
                            throw new CmisInvalidArgumentException("Extensions tree too deep! (More than " + XmlConstraints.MaxExtensionsDepth + " levels.)");
                        }

                        if (children == null)
                        {
                            children = new List<ICmisExtensionElement>();
                        }

                        if (children.Count + 1 > XmlConstraints.MaxExtensionsWidth)
                        {
                            throw new CmisInvalidArgumentException("Extensions tree too wide! (More than " + XmlConstraints.MaxExtensionsWidth + " extensions on one level.)");
                        }

                        children.Add(HandleExtensionLevel(parser, level + 1));

                        continue;
                    }

                    if (!XmlUtils.Next(parser))
                    {
                        break;
                    }
                }
            }

            XmlUtils.Next(parser);

            CmisExtensionElement element = new CmisExtensionElement()
            {
                Name = localname,
                Namespace = ns,
                Attributes = attributes
            };

            if (children != null)
            {
                element.Children = children;
            }
            else
            {
                element.Value = sb.ToString();
            }

            return element;
        }

        protected IList<S> AddToList<S>(IList<S> list, S value)
        {
            if (list == null)
            {
                list = new List<S>();
            }
            list.Add(value);

            return list;
        }

        protected string ReadText(XmlReader parser)
        {
            return XmlUtils.ReadText(parser, XmlConstraints.MaxStringLength);
        }

        protected bool? ReadBoolean(XmlReader parser)
        {
            string value = ReadText(parser);

            if (value == "true" || value == "1")
            {
                return true;
            }

            if (value == "false" || value == "0")
            {
                return false;
            }

            throw new CmisInvalidArgumentException("Invalid boolean value!");
        }

        protected BigInteger ReadInteger(XmlReader parser)
        {
            string value = ReadText(parser);

            try
            {
                return BigInteger.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);
            }
            catch (Exception e)
            {
                throw new CmisInvalidArgumentException("Invalid integer value!", e);
            }
        }

        protected decimal ReadDecimal(XmlReader parser)
        {
            string value = ReadText(parser);

            try
            {
                return Decimal.Parse(value, CultureInfo.InvariantCulture);
            }
            catch (Exception e)
            {
                throw new CmisInvalidArgumentException("Invalid decimal value!", e);
            }
        }

        protected DateTime ReadDateTime(XmlReader parser)
        {
            string value = ReadText(parser);

            try
            {
                return DateTimeHelper.ParseISO8601(value);
            }
            catch (Exception e)
            {
                throw new CmisInvalidArgumentException("Invalid datetime value!", e);
            }
        }

        protected E ReadEnum<E>(XmlReader parser)
        {
            return ReadText(parser).GetCmisEnum<E>();
        }

        protected abstract T PrepareTarget(XmlReader parser, string localname, string ns);

        protected abstract bool Read(XmlReader parser, string localname, string ns, T target);

    }

    internal static class XmlConstraints
    {
        public const int MaxStringLength = 100 * 1024;

        public const int MaxExtensionsWidth = 1000;
        public const int MaxExtensionsDepth = 100;
    }
}