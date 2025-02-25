using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using AlexCarvalho_Utils;

public class KnifeAttack : MonoBehaviour
{
    [SerializeField] float _swipeAngle;
    [SerializeField] float _swipeRange;
    [SerializeField] int _numberOfRaycasts;
    [SerializeField] float _damage;
    [SerializeField] Vector3 _startOffset;
    [SerializeField] GameObject _impactVisual;

    [Header("Repetition to make it feel more Responsive while moving")]
    [SerializeField] int _numberOfRecasts;
    [SerializeField] float _delayBetweenRecast;
    int _currentRecastIndex = 0;
    List<Transform> _dontRecastHit = new List<Transform>();

    List<Transform> possibleTargets = new List<Transform>();

    private PlayerFSM _playerState;


    private void Awake()
    {
        _playerState = GetComponent<PlayerFSM>();   
    }


    public void KnifeDetectTargets()
    {
        if (_currentRecastIndex >= _numberOfRecasts) 
        {
            _currentRecastIndex = 0;
            _dontRecastHit.Clear();
            return;
        }

        //Raycast In an Arc, add targets then do damage
        //First Detect Right side, then left side

        possibleTargets = My_Utils.ConeRaycast(_swipeAngle,transform.position + _startOffset,transform.forward,
            _swipeRange,_numberOfRaycasts);

        Debug.Log("Possible Targets Size is " + possibleTargets.Count); 

        //for (int i = 0; i < _numberOfRaycasts / 2; i++)
        //{
        //    Quaternion rotation = Quaternion.AngleAxis((_swipeAngle / _numberOfRaycasts) * i, Vector3.up);
        //    Vector3 rayCastDir = (rotation * transform.forward).normalized;
        //    RaycastHit hit;
        //    Ray ray = new Ray(transform.position + _startOffset, rayCastDir);

        //    if (Physics.Raycast(ray, out hit, _swipeRange))
        //    {
        //        possibleTargets.Add(hit.collider);
        //    }
        //}

        //for (int i = 0; i < _numberOfRaycasts / 2; i++)
        //{
        //    Quaternion rotation = Quaternion.AngleAxis((_swipeAngle / _numberOfRaycasts) * i, Vector3.up);
        //    Vector3 rayCastDir = (Quaternion.Inverse(rotation) * transform.forward).normalized;
        //    RaycastHit hit;
        //    Ray ray = new Ray(transform.position + _startOffset, rayCastDir);

        //    if (Physics.Raycast(ray, out hit, _swipeRange))
        //    {
        //        possibleTargets.Add(hit.collider);
        //    }
        //}
        FinishAttack();
        StartCoroutine(Recast());
    }

    private void FinishAttack()
    {
        //possibleTargets = possibleTargets.Distinct().ToList();

        foreach (var target in possibleTargets)
        {
            Debug.LogWarning("Name of colldier: " + target.name);
            DealDamage(target);
        }

        possibleTargets.Clear();
    }

    IEnumerator Recast()
    {
        yield return new WaitForSeconds(_delayBetweenRecast);
        KnifeDetectTargets();
        _currentRecastIndex++;
    }

    private void DealDamage(Transform other)
    {
        IHitable hitable = other.GetComponent<IHitable>();
        LootBox lootBox = other.GetComponent<LootBox>();
        if (hitable == null || _dontRecastHit.Contains(other)) return;

        //check if it's a loot box
        if (lootBox) { lootBox.InstantDestroy(); }
        //else handle Damage normally
        else 
        {
            hitable.HandleHit(new Damage(_damage));
            _dontRecastHit.Add(other);
        }
        Instantiate(_impactVisual, other.transform.position, Quaternion.LookRotation((other.transform.position - transform.position).normalized));
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < _numberOfRaycasts / 2; i++)
        {
            Quaternion rotation = Quaternion.AngleAxis((_swipeAngle / _numberOfRaycasts) * i, Vector3.up);
            Vector3 rayCastDir = (rotation * transform.forward);
            Debug.DrawRay(transform.position + _startOffset, rayCastDir * _swipeRange, Color.red);

        }
        for (int i = 0; i < _numberOfRaycasts / 2; i++)
        {
            Quaternion rotation = Quaternion.AngleAxis((_swipeAngle / _numberOfRaycasts) * i, Vector3.up);
            Vector3 rayCastDir = (Quaternion.Inverse(rotation) * transform.forward);
            Debug.DrawRay(transform.position + _startOffset, rayCastDir * _swipeRange, Color.red);

        }

    }

}

