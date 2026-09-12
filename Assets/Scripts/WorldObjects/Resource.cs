using ITF.Entity;
using UnityEngine;

namespace ITF.WorldObjects
{
    public class Resource : MapObject, IWorkPlace
    {
        [SerializeField] private Vector2[] workerSlotsPositions;
        [SerializeField] private float workingTime;
        private int freeNumberOfWorkerSlots;
        public int NumberOfWorkSlots => freeNumberOfWorkerSlots;

        public override void Init()
        {
            freeNumberOfWorkerSlots = workerSlotsPositions.Length;
        }
        public override void Deinit()
        {
            throw new System.NotImplementedException();
        }
        public void AffectWorker(Character worker)
        {

        }
    }
}
