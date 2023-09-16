using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.AIControllers
{
    public abstract class AIController : MonoBehaviour
    {
        public abstract string OutputString { get; set; }
        public abstract string[] OutputLines { get; set; }

        public abstract SceneDirector Director { get; set; }

        public abstract TextAsset SystemMessage { get; set; }

        public abstract void Init();
        public abstract void Clear();
        public abstract Task<string> EnterPromptAndGetResponse(string inputPrompt);
    }
}
