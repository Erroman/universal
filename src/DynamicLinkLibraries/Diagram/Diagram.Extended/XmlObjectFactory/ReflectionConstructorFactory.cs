﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Reflection;

using CategoryTheory;
using Diagram.UI.Interfaces;



namespace Diagram.UI.XmlObjectFactory
{
    /// <summary>
    /// Factory from refletion constructor
    /// </summary>
    public class ReflectionConstructorFactory //: AbstractXmlCreateObjectFactory
    {

        #region Fields

        Dictionary<string, Type> typeDictionary = null;
 
        #endregion

        #region Ctor


        #endregion

        #region Members

        /// <summary>
        /// Creates object from element
        /// </summary>
        /// <param name="type">Object type</param>
        /// <param name="element">The element</param>
        /// <returns>The object</returns>
        public ICategoryObject Create(string type, XmlElement element)
        {
            if (!typeDictionary.ContainsKey(type))
            {
                return null;
            }
            Type t = typeDictionary[type];
            ConstructorInfo c = t.GetConstructor(new Type[0]);
            return c.Invoke(new object[0]) as ICategoryObject;
        }

        #endregion

    }
}
