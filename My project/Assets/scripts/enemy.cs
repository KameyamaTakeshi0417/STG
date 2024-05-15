using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class enemy : MonoBehaviour
{
   public Vector3 shamblingWay;//�p�j���̃��[�g
    public float sight;//�v���C���[��F������͈�

    public float duration = 3.0f;//�ړ�����
    // �����J�E���^�[
    public float elapsedTime = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        float randomPos;//�����x�N�g����邽�߂̈ꎞ�I�Ȃ���
        randomPos= Random.Range(0.0f, 10.0f);
        shamblingWay.x = randomPos;
        randomPos = Random.Range(0.0f, 10.0f);
        shamblingWay.y=randomPos;

        shamblingWay += transform.position;
        elapsedTime = 0.0f;

        StartCoroutine(shambling(shamblingWay,duration*0.5f));//�ŏ��͕Г��Ȃ̂ňړ����Ԕ����B
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public float t;
    private IEnumerator shambling(Vector3 goalPos,float durationTime)
    {
        Vector3 startPos=transform.position;
        Vector3 endPos = goalPos;

        Vector3 playerPos;

        elapsedTime = 0.0f;
        playerPos =GameObject.Find("Player").transform.position;//�v���C���[�̈ʒu�m��
        
        while(true)
        {

            // �o�ߎ��Ԃ��X�V
            elapsedTime += Time.deltaTime;
            t = elapsedTime / durationTime;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            // �ړ��������������`�F�b�N
            if (t >= 1.0f)
            {
                break;//while���甲���o��
               
            }

            //�����ɍU�����[�`���J�n������
            if (getRangeToPlayer() >= sight)
            {
               // yield return LockOn(getWayToPlayer());
            }
            yield return new WaitForEndOfFrame();//1f�҂�
        }

        int count = 0;//�����J�E���^�[
        while (true) {
            count++;
            if (count >= 90){//90f�҂B�����͉��ƂȂ��B
                yield return shambling((transform.position + goalPos * (-2.0f)) , duration); //�v���C���[�����Ȃ�������ċA�I��shambling�Ăяo���B
            }
            yield return new WaitForEndOfFrame();
        }
       
    }
    private IEnumerator LockOn(Vector3 wayToPlayer) {
        //90f���炢�v���C���[�𒍎�����B
        int count= 0;
        while(true) {
            count++;
            //�v���C���[�����߂鏈���B�p�x���Ȃ񂩂���
            if (count >= 90) break;
        yield return new WaitForEndOfFrame();
        }

        //�͈͓��Ȃ�shoot�Ɉڍs�B
        if (getRangeToPlayer() > 0)
        {
            yield return shoot();
        }
        else
        {
            //�͈͊O�ɏo����shambling�ĊJ
            yield return shambling(shamblingWay, duration);//�����L�����B
        }

        yield return null;
    }
    private IEnumerator shoot() {
        //�e���ˁB3�񂭂炢�B�s�x10f�̃��b�N�I��(�R���[�`���Ăяo���łȂ��A�����œ��l�̏������L��)�A
        //�ˌ����x�𗎂Ƃ������̂�5f���炢�ҋ@�A���ˁB�v���C���[�̈ʒu���킸�A3��J��Ԃ��B
        //3���ł��I�������10f�҂B
        //�͈͓��Ƀv���C���[��������shoot�ɖ߂�B
        //�͈͊O�ɏo����shambling�ĊJ�B
        yield return shambling(shamblingWay, duration); //�v���C���[�����Ȃ�������ċA�I��shambling�Ăяo���B

    }
    private Vector3 getWayToPlayer() {
        Vector3 ret;
        ret= transform.position - GameObject.Find("Player").transform.position;
        return ret;
    }
    private float getRangeToPlayer() {
        float ret=0.0f;
        Vector3 length;
        //�v���C���[�Ƃ̋������x�N�g���ō���āA�͈͓��Ȃ�ret�Ԃ��B�����łȂ��Ȃ�}�C�i�X�Ԃ��B
        length = getWayToPlayer();
        ret=length.magnitude;
        ret=Mathf.Abs(ret);//�}�C�i�X����Ȃ�
        if(ret>=sight)return ret;
        return -1.0f;
    }
}
