
using Obi;
using Unity.Netcode;
using UnityEngine;


public class ObiSpawner : NetworkBehaviour
{
 
    [SerializeField] private ObiSolver solver;
    [SerializeField] private Material ropeMaterial;

    [SerializeField] private int numParticles = 20;
    [SerializeField] private bool fixedParticleCount = true;

    [SerializeField] private ObiRopeBlueprint _blueprint;



    public void SpawnObiRope(Transform objec1, Transform objec2){
        if (objec1 == null || objec2 == null) return;

            float yOffset = 1.035f;
            Vector3 startPos = objec1.position + new Vector3(0, yOffset, 0);
            Vector3 endPos = objec2.position + new Vector3(0, yOffset, 0);


            transform.position = (startPos + endPos) / 2;
            transform.rotation = Quaternion.FromToRotation(Vector3.right, startPos - endPos);

            Vector3 startPositionLS = transform.InverseTransformPoint(startPos);
            Vector3 endPositionLS = transform.InverseTransformPoint(endPos);
            Vector3 tangentLS = (endPositionLS - startPositionLS).normalized;


            var blueprint = _blueprint;


            int filter = ObiUtils.MakeFilter(ObiUtils.CollideWithEverything, 0);
            blueprint.path.Clear();
            blueprint.path.AddControlPoint(startPositionLS, -tangentLS, tangentLS, Vector3.up, 0.1f, 0.1f, 1, filter, Color.white, "start");
            blueprint.path.AddControlPoint(endPositionLS, -tangentLS, tangentLS, Vector3.up, 0.1f, 0.1f, 1, filter, Color.white, "end");
            blueprint.path.FlushEvents();

            if (fixedParticleCount)
                blueprint.resolution = numParticles / (blueprint.path.Length / blueprint.thickness);

            blueprint.GenerateImmediate();

            var rope = gameObject.AddComponent<ObiRope>();
            var ropeRenderer = gameObject.AddComponent<ObiRopeExtrudedRenderer>();
            var attachment1 = gameObject.AddComponent<ObiParticleAttachment>();
            var attachment2 = gameObject.AddComponent<ObiParticleAttachment>();

            ropeRenderer.section = Resources.Load<ObiRopeSection>("DefaultRopeSection");
            ropeRenderer.material = ropeMaterial;

            rope.ropeBlueprint = blueprint;

            // attachment1.attachmentType = ObiParticleAttachment.AttachmentType.Dynamic;
            // attachment2.attachmentType = ObiParticleAttachment.AttachmentType.Dynamic;
            attachment1.target = objec1;
            attachment2.target = objec2;
            attachment1.particleGroup = blueprint.groups[0];
            attachment2.particleGroup = blueprint.groups[1];


            
            transform.SetParent(solver.transform);
        

    }


   



}
