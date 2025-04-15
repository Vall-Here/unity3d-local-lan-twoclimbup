using UnityEngine;

public class FlingToFinish : MonoBehaviour
{
    public CharacterController player1;        // Karakter pertama
    public CharacterController player2;        // Karakter kedua
    public float ropeLength = 10f;             // Panjang tali saat tidak ditarik
    public float springConstant = 50f;         // Kekuatan elastisitas tali (seperti kekakuan pegas)
    public float damping = 0.1f;               // Redaman untuk menghindari osilasi berlebihan
    public float gravityMultiplier = 1f;       // Pengaturan gaya gravitasi tambahan
    public float maxStretch = 20f;             // Jarak maksimum yang bisa ditarik oleh tali
    public float flingStrength = 30f;          // Kekuatan lemparan karakter
    public float flingTime = 0.5f;             // Waktu delay untuk "fling" atau "lempar"

    private Vector3 velocity1 = Vector3.zero;  // Kecepatan karakter 1
    private Vector3 velocity2 = Vector3.zero;  // Kecepatan karakter 2
    private Vector3 previousPosition1;         // Posisi sebelumnya untuk player1
    private Vector3 previousPosition2;         // Posisi sebelumnya untuk player2
    private bool isFlinging = false;           // Status apakah sedang dalam kondisi fling

    void Start()
    {
        previousPosition1 = player1.transform.position;
        previousPosition2 = player2.transform.position;
    }

    void Update()
    {
        // Hitung jarak dan vektor tarik antara player1 dan player2
        Vector3 direction = player2.transform.position - player1.transform.position;
        float distance = direction.magnitude;

        // Hitung gaya tarik berdasarkan perpanjangan dari panjang tali
        float stretch = Mathf.Clamp(distance - ropeLength, 0f, maxStretch);
        Vector3 springForce = direction.normalized * (stretch * springConstant);

        // Gaya gravitasi (karena kita tidak menggunakan Rigidbody, kita hitung manual)
        Vector3 gravityForce1 = Vector3.down * gravityMultiplier * 9.81f;  // Percepatan gravitasi di bumi
        Vector3 gravityForce2 = Vector3.down * gravityMultiplier * 9.81f;

        // Terapkan gaya pegas ke player1 dan player2
        ApplyForceToCharacter(player1, springForce + gravityForce1);
        ApplyForceToCharacter(player2, -springForce + gravityForce2);

        // Terapkan redaman agar tali tidak osilasi berlebihan
        Vector3 relativeVelocity = velocity2 - velocity1;
        Vector3 dampingForce = relativeVelocity * damping;

        // Terapkan gaya redaman ke kedua karakter
        ApplyForceToCharacter(player1, dampingForce);
        ApplyForceToCharacter(player2, -dampingForce);

        // Lakukan fling (lemparan) jika kondisi terpenuhi
        if (isFlinging)
        {
            FlingCharacters();
        }

        // Update posisi sebelumnya untuk menghitung kecepatan relatif
        velocity1 = (player1.transform.position - previousPosition1) / Time.deltaTime;
        velocity2 = (player2.transform.position - previousPosition2) / Time.deltaTime;

        previousPosition1 = player1.transform.position;
        previousPosition2 = player2.transform.position;
    }

    private void ApplyForceToCharacter(CharacterController character, Vector3 force)
    {
        // Karena tidak ada Rigidbody, kita harus menghitung pergerakan manual dengan menggunakan Move
        Vector3 movement = force * Time.deltaTime;

        // Gunakan fungsi Move untuk menerapkan pergerakan pada CharacterController
        character.Move(movement);
    }

    private void FlingCharacters()
    {
        // Lakukan fling untuk kedua karakter
        Vector3 flingDirection1 = (player2.transform.position - player1.transform.position).normalized;
        Vector3 flingDirection2 = (player1.transform.position - player2.transform.position).normalized;

        // Terapkan gaya lemparan ke karakter
        player1.Move(flingDirection1 * flingStrength * Time.deltaTime);
        player2.Move(flingDirection2 * flingStrength * Time.deltaTime);
        
        // Matikan fling setelah waktu tertentu
        Invoke("StopFling", flingTime);
    }

    private void StopFling()
    {
        isFlinging = false;
    }

    public void StartFling()
    {
        isFlinging = true;
    }
}
