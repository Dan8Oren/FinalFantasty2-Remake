using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Mechanics.FightScene
{
    public class EnemyBehavior
    {
        private List<CharacterDisplayScript> _allFighters;
        private CharacterDisplayScript _curEnemyFighter;
        public EnemyBehavior(List<CharacterDisplayScript> allFighters)
        {
            _allFighters = allFighters;
        }
        /**
     * makes a randomize enemy action by his attacks and magics
     */
    public void MakeEnemyAction(CharacterDisplayScript curFighter)
        {
            _curEnemyFighter = curFighter;
            var fighter = curFighter.data;
            var damage = RandomizeEnemyAttack(fighter, fighter.magics, fighter.attacks);
            if (damage == 0) return;
            var attackIndex = Random.Range(FightManager.k_NUM_OF_HEROES, _allFighters.Count);
            while (!_allFighters[attackIndex].isActiveAndEnabled) attackIndex = Random.Range(FightManager.k_NUM_OF_HEROES, _allFighters.Count);
            var heroToAttack = _allFighters[attackIndex];
            FightManager.Instance.actionsLogScript.AddToLog($" on '{heroToAttack.data.displayName}' for {Mathf.Abs(damage)} points");
            heroToAttack.EffectHealth(damage);
        }

    /**
     * gets a random enemy attack and initializes it if no hero needs to be selected.
     * other wise return the attack pointsOfEffect.
     * <returns> The chosen attack's calculated pointsOfEffect or zero if attack all-ready accord. </returns>
     * >
     */
    private int RandomizeEnemyAttack(CharacterData fighter,
        MagicAttackData[] magics, RegularAttackData[] attacks)
    {
        bool isOnSelf = false, effectAllEnemyGroup = false;
        var magicOrAttack = Random.Range(0, 2);
        if (fighter.magics.Length == 0) magicOrAttack = 0;

        if (fighter.attacks.Length == 0) magicOrAttack = 1;
        var damage = 0;
        if (magicOrAttack == 0)
        {
            var actionInd = Random.Range(0, attacks.Length);
            FightManager.Instance.actionsLogScript.AddToLog($" Uses {attacks[actionInd].displayName}");
            damage = fighter.CalculateMeleeDamage(attacks[actionInd]);
            effectAllEnemyGroup = attacks[actionInd].effectAllEnemyGroup;
        }
        else
        {
            var actionInd = Random.Range(0, magics.Length);
            FightManager.Instance.actionsLogScript.AddToLog($" Uses {magics[actionInd].displayName}");
            damage = fighter.CalculateMagicEffect(magics[actionInd]);
            isOnSelf = magics[actionInd].isOnSelf;
            effectAllEnemyGroup = magics[actionInd].effectAllEnemyGroup;
        }

        if (HandleNoTargetEnemyAction(isOnSelf, effectAllEnemyGroup, damage,fighter)) return 0;

        return damage;
    }

    private bool HandleNoTargetEnemyAction(bool isOnSelf, bool effectAllEnemyGroup, int damage,CharacterData fighter)
    {
        FightManager fightManager = FightManager.Instance;
        List <CharacterDisplayScript> charactersFightOrder = fightManager.GetFightOrder();
        if (isOnSelf)
        {
            fightManager.actionsLogScript.AddToLog($" for {Mathf.Abs(damage)}");
            _curEnemyFighter.EffectHealth(damage);
            return true;
        }

        if (effectAllEnemyGroup)
        {
            fightManager.actionsLogScript.AddToLog($" on all heroes for {Mathf.Abs(damage)} points");
            foreach (var character in charactersFightOrder)
                if (character.data.isHero)
                    character.EffectHealth(damage);
            return true;
        }

        return false;
    }
    }
}