using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace WeaverNet.Mod.Game
{
    public class EnemyInstance
    {
        public string Name { get; }
        public int Id { get; }
        public GameObject GameObject { get; }
        public HealthManager HealthManager { get; }
        public Rigidbody2D Rigidbody2D { get; }
        public PlayMakerFSM[] PlayMakers { get; }
        public EnemyInstance(string name, int id, GameObject gameObject, HealthManager healthManager, Rigidbody2D rigidbody2D, PlayMakerFSM[] playMakers)
        {
            Name = name;
            Id = id;
            GameObject = gameObject;
            HealthManager = healthManager;
            Rigidbody2D = rigidbody2D;
            PlayMakers = playMakers;
        }
    }
}
