using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Rigidbody2D rb;

    public float HP;
    public float Pow;
    public float moveSpeed;
    public GameObject bullet;
    public float bulletSpeed;
    public float BulletSpan;
    public Vector3 watch;

    // Start is called before the first frame update
    void Start()
    {
       // bullet = Instantiate(bullet, transform.position, Quaternion.identity);

        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        // ���͂̊p�x���擾����
        float moveAngle = Mathf.Atan2(Input.GetAxis("Vertical"), Input.GetAxis("Horizontal")) * Mathf.Rad2Deg;
        watch = new Vector2(Mathf.Cos(transform.rotation.eulerAngles.z * Mathf.Deg2Rad), Mathf.Sin(transform.rotation.eulerAngles.z * Mathf.Deg2Rad));
        // ���͂��Ȃ��ꍇ�͉������Ȃ�
        if (Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") == 0)
        {
            rb.velocity = Vector2.zero;
            return;
        }
        if (Input.GetKeyDown(KeyCode.Space)) {
            GameObject bulletPrefab = Instantiate(bullet, transform.position, Quaternion.identity);
            bulletPrefab.GetComponent<Bullet_Base>().setRotate(watch);
            Destroy(bulletPrefab, 3);
        }
            // �L�����N�^�[���ړ�������
            rb.velocity = Quaternion.AngleAxis(moveAngle, Vector3.forward) * Vector2.right * moveSpeed;

        // �L�����N�^�[�̌�����ς���
        transform.rotation = Quaternion.AngleAxis(moveAngle, Vector3.forward);
    }
}
