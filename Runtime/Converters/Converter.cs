﻿using UnityEngine;

namespace DimensionConverter
{
    public abstract class Converter : MonoBehaviour
    {
        private bool started = false;

        protected virtual void OnEnable()
        {
            Dimension.AddConverter(this);
            if(started)
            {
                Convert(Dimension.is2DNot3D);
            }
        }

        protected virtual void Start()
        {
            Convert(Dimension.is2DNot3D);
            started = true;
        }

        protected virtual void OnDisable()
        {
            Dimension.RemoveConverter(this);
        }

        /// <include file='../Documentation.xml' path='docs/Converter/Convert/*'/>
        public void Convert(bool to2DNot3D)
        {
            if(to2DNot3D)
            {
                ConvertTo2D();
            }
            else
            {
                ConvertTo3D();
            }
        }

        /// <include file='../Documentation.xml' path='docs/Converter/ConvertTo2D/*'/>
        public abstract void ConvertTo2D();
        /// <include file='../Documentation.xml' path='docs/Converter/ConvertTo3D/*'/>
        public abstract void ConvertTo3D();
    }
}
