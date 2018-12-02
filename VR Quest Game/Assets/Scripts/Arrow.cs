using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Arrow : NetworkBehaviour {

    //fields
    public GameObject ExplosionPrefab;

    private float speed;
    private ParticipantID shooter;
    private WaitForSecondsRealtime lifeTime;
    private float lifeTimeSeconds;
    private Ability ability;
    private GameObject abilityIndicator;
    private bool abilityIsTriggered;
    private bool arrowIsShot;

    //properties
    public float Speed { get { return this.speed; } }
    public Ability Ability { get { return this.ability; } }

    //methods
    private void Awake()
    {
        abilityIndicator = this.transform.GetChild(0).gameObject;
    }
    [Command]
    private void CmdSetAbility(Ability a)
    {
        SetAbilityLocal(a);
        RpcSetAbility(a);
    }
    [ClientRpc]
    private void RpcSetAbility(Ability a)
    {
        if (!isLocalPlayer)
        {
            SetAbilityLocal(a);
        }
    }
    public void SetAbility(Ability a)
    {
        if (isLocalPlayer)
        {
            SetAbilityLocal(a);
            CmdSetAbility(a);
        }
    }
    private void SetAbilityLocal(Ability a)
    {
        ability = a;
        if (a == Ability.None)
        {
            abilityIndicator.SetActive(false);
        }
        else
        {
            abilityIndicator.SetActive(true);
            if (a == Ability.Teleport)
            {
                abilityIndicator.GetComponent<MeshRenderer>().material.color = Color.yellow;
            }
            else if (a == Ability.Explosive)
            {
                abilityIndicator.GetComponent<MeshRenderer>().material.color = Color.red;
            }
            else if (a == Ability.Mega)
            {
                abilityIndicator.GetComponent<MeshRenderer>().material.color = Color.black;
            }
        }
    } 
    [Command]
    private void CmdTriggerAbility()
    {
        RpcTriggerAbility();
    } 
    [ClientRpc]
    private void RpcTriggerAbility()
    {
        if (!isLocalPlayer)
        {
            triggerAbilityLocal();
        }
    }
    public void TriggerAbility()
    {
        if (isLocalPlayer)
        {
            triggerAbilityLocal();
            CmdTriggerAbility();
        }
    }
    private void triggerAbilityLocal()
    {
        if (!abilityIsTriggered)
        {
            if (ability == Ability.Explosive)
            {
                abilityIsTriggered = true;
                arrowIsShot = true; //in order to kill the shooter as well
                speed = 0;
                GameObject explosive = GameObject.Instantiate(ExplosionPrefab, this.transform.position, Quaternion.Euler(Vector3.zero));
                explosive.GetComponent<Renderer>().material = this.GetComponent<Renderer>().material;
                explosive.transform.parent = this.transform;
                StartCoroutine("explode", explosive);
            }
            else if (ability == Ability.Mega)
            {
                abilityIsTriggered = true;
                StartCoroutine("mega");
            }
        }
    }
    [Server]
    private void destroyArrow() 
    {
        StopAllCoroutines();
        NetworkServer.Destroy(this.gameObject);
    }
    [Server]
    public void SetShooter(ParticipantID id)
    {
        if (this.shooter == null)
        {
            this.shooter = id;
        }
    }
    public void SetArrow(float Speed, float lifeTimeOfArrow)
    {
        if(this.speed <= 0)
        {
            this.speed = Speed / this.transform.localScale.z; //World speed --> local speed
            arrowIsShot = true;
            if (isServer)
            {
                lifeTimeSeconds = lifeTimeOfArrow;
                StartCoroutine("lifeTimeCounter");
            }
        }
    }
    [Server]
    private ParticipantID reportHit(GameObject hit, bool instantKill)
    {
        if (shooter != null)
        {
            int damage = 0;
            ParticipantID hitID = null;

            if (hit.name.Contains("Head")) //headshot
            {
                damage = 2;
            }
            else if (hit.name.Contains("Body")) //bodyshot
            {
                damage = 1;
                if (instantKill)
                {
                    damage = 2;
                }
            }
            if (damage > 0)
            {
                hitID = ParticipantHelper.PH.getIDByBodyPart(hit);
                if (hitID != null && !(hitID == shooter && !arrowIsShot))
                {
                    ParticipantHelper.PH.ReportArrowHit(shooter, hitID, damage);
                    return hitID;
                }
                return null; //if hitID == shooter && !arrowIsShot change hitID back to null!
            }
            return null;
        }
        return null;
    }  
    private IEnumerator mega()
    {
        if (isServer)
        {
            StopCoroutine("lifeTimeCounter");
            lifeTimeSeconds *= 5;
            StartCoroutine("lifeTimeCounter");
        }

        //animation
        Vector3 currentScale = this.transform.localScale;
        Vector3 destinationScale = currentScale *10;
        float timeIncrease = 0.5f; //explode time = 1 second / timeIncrease
        float t = 0;

        while (t < 1) //release string
        {
            t += Time.deltaTime * timeIncrease;
            this.transform.localScale = Vector3.Lerp(currentScale, destinationScale, t);
            yield return null;
        }
    }
    private IEnumerator explode(GameObject explosive)
    {
        if (isServer)
        {
            StopCoroutine("lifeTimeCounter");
        }

        //animation
        Vector3 currentScale = explosive.transform.localScale;
        Vector3 destinationScale = currentScale * 8;
        int timeIncrease = 1; //explode time = 1 second / timeIncrease
        float t = 0;

        while (t < 1) //release string
        {
            t += Time.deltaTime * timeIncrease;
            explosive.transform.localScale = Vector3.Lerp(currentScale, destinationScale, t);
            yield return null;
        }

        //killing participants
        if (isServer)
        {
            float radius = explosive.GetComponent<Renderer>().bounds.size.z / 2;
            Collider[] hits = Physics.OverlapSphere(this.transform.position, radius);
            for (int i = 0; i < hits.Length; i++)
            {
                reportHit(hits[i].gameObject, true);
            }
            yield return new WaitForSecondsRealtime(1);
            destroyArrow();
        }       
    }
    private IEnumerator lifeTimeCounter()
    {
        lifeTime = new WaitForSecondsRealtime(lifeTimeSeconds);
        yield return lifeTime;
        destroyArrow();
    }
    [Server]
    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.name.Contains("Arrow"))
        {
            if (ability == Ability.None || (ability == Ability.Mega && !abilityIsTriggered))
            {
                if (reportHit(other.gameObject, false) != null)
                {
                    if (arrowIsShot)
                    {
                        destroyArrow();
                    }
                }
                else if (arrowIsShot)
                {
                    destroyArrow();
                }
            }
            else if (ability == Ability.Explosive && !abilityIsTriggered)
            {
                if (reportHit(other.gameObject, false) != null)
                {
                    TriggerAbility(); //destroys itself
                }
                else if (arrowIsShot)
                {
                    TriggerAbility();
                }
            }
            else if (ability == Ability.Mega && abilityIsTriggered)
            {
                reportHit(other.gameObject, true);
            }
        }
        
    }
}
