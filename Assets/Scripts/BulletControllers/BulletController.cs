﻿using Joule.CharacterControllers;
using Joule.Events.BulletControllers;
using UnityEngine;
using UnityEngine.Assertions;

namespace Joule.BulletControllers
{
    /// <summary>
    /// 弾を制御するクラス
    /// </summary>
    public sealed class BulletController : MonoBehaviour
    {
        [SerializeField]
        private float duration;

        [SerializeField]
        private float moveSpeed;

        [SerializeField]
        private int damage;
        public int Damage
        {
            get { return damage; }
            set { damage = value; }
        }

        /// <summary>
        /// 貫通できる回数
        /// </summary>
        [SerializeField]
        private int penetration;

        private Character owner;

        private Transform cachedTransform;

        void Awake()
        {
            this.cachedTransform = this.transform;
        }

        void Update()
        {
            this.cachedTransform.position += this.cachedTransform.forward * this.moveSpeed * Time.deltaTime;
        }

        public void Initialize(Character owner)
        {
            this.owner = owner;
            Destroy(this.gameObject, this.duration);
        }
        
        private void OnTriggerEnter(Collider other)
        {
            var character = other.GetComponentInParent<Character>();

            if (character == null)
            {
                return;
            }
            
            character.Broker.Publish(HitBullet.Get(this));
            this.penetration--;
            if (this.penetration <= 0)
            {
                Destroy(this.gameObject);
            }
        }
    }
}
