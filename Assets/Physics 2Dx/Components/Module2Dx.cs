﻿using UnityEngine;

namespace Physics2DxSystem
{
    public abstract class Module2Dx : MonoBehaviour
    {
        protected virtual void OnEnable()
        {
            Physics2Dx.AddModule2DxInstance(this);

            Convert(Physics2Dx.is2Dnot3D);
        }

        protected virtual void OnDisable()
        {
            Physics2Dx.RemoveModule2DxInstance(this);
        }

        /// <include file='../Documentation.xml' path='docs/Modules/Module2Dx/Convert/*'/>
        public void Convert(bool to2Dnot3D)
        {
            if(to2Dnot3D)
            {
                ConvertTo2D();
            }
            else
            {
                ConvertTo3D();
            }
        }

        /// <include file='../Documentation.xml' path='docs/Modules/Module2Dx/ConvertTo2D/*'/>
        public abstract void ConvertTo2D();
        /// <include file='../Documentation.xml' path='docs/Modules/Module2Dx/ConvertTo3D/*'/>
        public abstract void ConvertTo3D();
    }
}
