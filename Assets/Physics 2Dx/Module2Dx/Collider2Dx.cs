﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Physics2DxSystem
{
    [RequireComponent(typeof(Transform2Dx))]
    public abstract class Collider2Dx<C, C2D> : Module2Dx where C : Collider where C2D : Collider2D
    {
        #region Required Components
        private Transform2Dx _transform2Dx;
        public Transform2Dx transform2Dx => _transform2Dx ? _transform2Dx : (_transform2Dx = GetComponent<Transform2Dx>());
        #endregion

        #region Properties
        [Tooltip("Automatically update the tracked colliders by looking for dynamically added and removed colliders. To improve performance, turn this setting off and use the Add / Remove methods of this Component instead.")] public bool autoUpdate = true;
        [Tooltip("The Colliders to ignore for conversion.")] public C[] ignoredColliders;
        [Tooltip("The Collider2Ds to ignore for conversion.")] public C2D[] ignoredColliders2D;

        public bool IgnoreCollider(C collider) => Array.IndexOf(ignoredColliders, collider) != -1;
        public bool IgnoreCollider2D(C2D collider2D) => Array.IndexOf(ignoredColliders2D, collider2D) != -1;
        #endregion
        
        #region Tracked Colliders
        private List<C> trackedColliders = new List<C>();
        private List<C2D> trackedCollider2Ds = new List<C2D>();

        public int collidersCount => trackedColliders.Count;
        public int collider2DsCount => trackedCollider2Ds.Count;

        public ReadOnlyCollection<C> colliders => trackedColliders.AsReadOnly();
        public ReadOnlyCollection<C2D> collider2Ds => trackedCollider2Ds.AsReadOnly();

        private void AddPair(C collider, C2D collider2D)
        {
            trackedColliders.Add(collider);
            trackedCollider2Ds.Add(collider2D);
        }

        public C GetColliderAt(int index) => trackedColliders[index];
        public C2D GetCollider2DAt(int index) => trackedCollider2Ds[index];

        public int GetColliderIndex(C collider) => trackedColliders.IndexOf(collider);
        public int GetCollider2DIndex(C2D collider2D) => trackedCollider2Ds.IndexOf(collider2D);

        /// <include file='../Documentation.xml' path='docs/Collider2Dx/GetPairedCollider2D/*' />
        public C2D GetPairedCollider2D(C collider)
        {
            var index = GetColliderIndex(collider);
            return index != -1 ? GetCollider2DAt(index) : null;
        }

        /// <include file='../Documentation.xml' path='docs/Collider2Dx/GetPairedCollider/*' />
        public C GetPairedCollider(C2D collider2D)
        {
            var index = GetCollider2DIndex(collider2D);
            return index != -1 ? GetColliderAt(index) : null;
        }

        /// <include file='../Documentation.xml' path='docs/Collider2Dx/ColliderToCollider/*' />
        protected abstract void ColliderToCollider(C collider, C other);
        /// <include file='../Documentation.xml' path='docs/Collider2Dx/Collider2DToCollider2D/*' />
        protected abstract void Collider2DToCollider2D(C2D collider2D, C2D other);

        /// <include file='../Documentation.xml' path='docs/Collider2Dx/AddCollider/*' />
        public C AddCollider()
        {
            var collider = transform2Dx.gameObject3D.AddComponent<C>();
            var collider2D = transform2Dx.gameObject2D.AddComponent<C2D>();

            AddPair(collider, collider2D);
            return collider;
        }

        /// <include file='../Documentation.xml' path='docs/Collider2Dx/AddCollider2D/*' />
        public C2D AddCollider2D()
        {
            var collider2D = transform2Dx.gameObject2D.AddComponent<C2D>();
            var collider = transform2Dx.gameObject3D.AddComponent<C>();

            AddPair(collider, collider2D);
            return collider2D;
        }

        /// <include file='../Documentation.xml' path='docs/Collider2Dx/AddCollider/*' />
        public C AddCollider(C copyOf)
        {
            var collider = AddCollider();

            ColliderToCollider(copyOf, collider);
            if(Physics2Dx.is2DNot3D)
            {
                var collider2D = trackedCollider2Ds[trackedCollider2Ds.Count - 1];
                ColliderToCollider2D(collider, collider2D);
            }

            return collider;
        }

        /// <include file='../Documentation.xml' path='docs/Collider2Dx/AddCollider2D/*' />
        public C2D AddCollider2D(C2D copyOf)
        {
            var collider2D = AddCollider2D();

            Collider2DToCollider2D(copyOf, collider2D);
            if(!Physics2Dx.is2DNot3D)
            {
                var collider = trackedColliders[trackedColliders.Count - 1];
                Collider2DToCollider(collider2D, collider);
            }

            return collider2D;
        }

        /// <include file='../Documentation.xml' path='docs/Collider2Dx/DestroyPairedColliders/*' />
        public void DestroyPairedColliders(C collider) => DestroyPairedCollidersAt(GetColliderIndex(collider));
        /// <include file='../Documentation.xml' path='docs/Collider2Dx/DestroyPairedColliders/*' />
        public void DestroyPairedColliders(C2D collider2D) => DestroyPairedCollidersAt(GetCollider2DIndex(collider2D));

        /// <include file='../Documentation.xml' path='docs/Collider2Dx/DestroyPairedColliders/*' />
        public void DestroyPairedCollidersAt(int index)
        {
            Destroy(trackedColliders[index]);
            Destroy(trackedCollider2Ds[index]);

            trackedColliders.RemoveAt(index);
            trackedCollider2Ds.RemoveAt(index);
        }
        #endregion

        #region Unity Methods
        protected virtual void Awake()
        {
            CacheColliders();
            CacheCollider2Ds();

            var tempAutoUpdate = autoUpdate;
            autoUpdate = false;
            if(Physics2Dx.is2DNot3D)
            {
                ConvertTo3D();
            }
            else
            {
                ConvertTo2D();
            }
            autoUpdate = tempAutoUpdate;
        }
        #endregion

        #region Collider Updating
        /// <include file='../Documentation.xml' path='docs/Collider2Dx/UpdateColliders/*' />
        public void UpdateColliders()
        {
            C collider;
            C2D collider2D;
            for(int i = trackedColliders.Count - 1; i >= 0; i--)
            {
                collider = trackedColliders[i];
                collider2D = trackedCollider2Ds[i];

                // If the Collider is missing, remove it and destroy the associated Collider2D.
                if(!collider)
                {
                    trackedColliders.RemoveAt(i);
                    trackedCollider2Ds.RemoveAt(i);
                    Destroy(collider2D);
                }
                // If the Collider2D is missing, create a new one.
                else if(!collider2D)
                {
                    trackedCollider2Ds[i] = transform2Dx.gameObject2D.AddComponent<C2D>();
                }
            }
        }

        /// <include file='../Documentation.xml' path='docs/Collider2Dx/UpdateCollider2Ds/*' />
        public void UpdateCollider2Ds()
        {
            C2D collider2D;
            C collider;
            for(int i = trackedCollider2Ds.Count - 1; i >= 0; i--)
            {
                collider2D = trackedCollider2Ds[i];
                collider = trackedColliders[i];

                // If the Collider2D is missing, remove it and destroy the associated Collider.
                if(!collider2D)
                {
                    trackedCollider2Ds.RemoveAt(i);
                    trackedColliders.RemoveAt(i);
                    Destroy(collider);
                }
                // If the Collider is missing, create a new one.
                else if(!collider)
                {
                    trackedColliders[i] = transform2Dx.gameObject3D.AddComponent<C>();
                }
            }
        }

        /// <include file='../Documentation.xml' path='docs/Collider2Dx/CacheColliders/*' />
        public virtual void CacheColliders()
        {
            foreach(var collider in transform2Dx.gameObject3D.GetComponents<C>())
            {
                if(!trackedColliders.Contains(collider) && !IgnoreCollider(collider))
                {
                    var collider2D = transform2Dx.gameObject2D.AddComponent<C2D>();
                    if(Physics2Dx.is2DNot3D)
                    {
                        ColliderToCollider2D(collider, collider2D);
                    }

                    AddPair(collider, collider2D);
                }
            }

            foreach(var collider in GetComponents<C>())
            {
                if(!IgnoreCollider(collider))
                {
                    AddCollider(collider);
                    DestroyImmediate(collider);
                }
            }
        }

        /// <include file='../Documentation.xml' path='docs/Collider2Dx/CacheCollider2Ds/*' />
        public virtual void CacheCollider2Ds()
        {
            foreach(var collider2D in transform2Dx.gameObject2D.GetComponents<C2D>())
            {
                if(!trackedCollider2Ds.Contains(collider2D) && !IgnoreCollider2D(collider2D))
                {
                    var collider = transform2Dx.gameObject3D.AddComponent<C>();
                    if(!Physics2Dx.is2DNot3D)
                    {
                        Collider2DToCollider(collider2D, collider);
                    }

                    AddPair(collider, collider2D);
                }
            }

            foreach(var collider2D in GetComponents<C2D>())
            {
                if(!IgnoreCollider2D(collider2D))
                {
                    AddCollider2D(collider2D);
                    DestroyImmediate(collider2D);
                }
            }
        }
        #endregion

        #region Conversion
        /// <include file='../Documentation.xml' path='docs/Collider2Dx/ColliderToCollider2D/*' />
        protected abstract void ColliderToCollider2D(C collider, C2D collider2D);
        /// <include file='../Documentation.xml' path='docs/Collider2Dx/Collider2DToCollider/*' />
        protected abstract void Collider2DToCollider(C2D collider2D, C collider);

        public override void ConvertTo2D()
        {
            if(autoUpdate)
            {
                UpdateColliders();
                CacheColliders();
            }

            for(int i = 0; i < trackedColliders.Count; i++)
            {
                ColliderToCollider2D(trackedColliders[i], trackedCollider2Ds[i]);
            }
        }

        public override void ConvertTo3D()
        {
            if(autoUpdate)
            {
                UpdateCollider2Ds();
                CacheCollider2Ds();
            }

            for(int i = 0; i < trackedCollider2Ds.Count; i++)
            {
                Collider2DToCollider(trackedCollider2Ds[i], trackedColliders[i]);
            }
        }
        #endregion
    }
}
