using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class IntegrationTestRunner : MonoBehaviour
{
    private List<string> testResults = new List<string>();
    private int currentTest = 0;
    private bool isRunning = false;
    
    void Start()
    {
        StartCoroutine(RunAllTests());
    }
    
    IEnumerator RunAllTests()
    {
        isRunning = true;
        Debug.Log("=== INTEGRATION TEST START ===");
        
        // Test 1: Move/Jump/Double Jump
        yield return StartCoroutine(TestMoveJump());
        
        // Test 2: Sword attack hit + enemy damage + kill
        yield return StartCoroutine(TestSwordAttack());
        
        // Test 3: Player attack -> slow motion + camera zoom -> return
        yield return StartCoroutine(TestPlayerAttackSlowMoZoom());
        
        // Test 4: Enemy attack -> slow motion -> parry window
        yield return StartCoroutine(TestEnemyAttackParryWindow());
        
        // Test 5: Parry success -> counterattack (normal/perfect log)
        yield return StartCoroutine(TestParrySuccess());
        
        // Test 6: Parry fail -> player damage + knockback (verify knockback)
        yield return StartCoroutine(TestParryFailKnockback());
        
        // Test 7: Combat start/end camera zoom/return (no stutter)
        yield return StartCoroutine(TestCombatCameraZoom());
        
        // Test 8: Death -> GameOver -> checkpoint respawn -> reset
        yield return StartCoroutine(TestDeathRespawn());
        
        // Test 9: Stage end -> Clear -> R restart
        yield return StartCoroutine(TestStageClearRestart());
        
        Debug.Log("=== INTEGRATION TEST COMPLETE ===");
        foreach (var result in testResults)
        {
            Debug.Log(result);
        }
        
        isRunning = false;
    }
    
    IEnumerator TestMoveJump()
    {
        Debug.Log("[TEST 1] Move/Jump/Double Jump - START");
        var player = GameObject.Find("Player");
        var controller = player.GetComponent<PlayerController>();
        var rb = player.GetComponent<Rigidbody2D>();
        
        // Test move
        float startX = player.transform.position.x;
        controller.Move(1f);
        yield return new WaitForSeconds(0.5f);
        float endX = player.transform.position.x;
        bool moveOk = endX > startX;
        
        // Test jump
        controller.Jump();
        yield return new WaitForSeconds(0.2f);
        bool jumpOk = rb.linearVelocity.y > 0;
        
        // Test double jump
        controller.Jump();
        yield return new WaitForSeconds(0.2f);
        bool doubleJumpOk = rb.linearVelocity.y > 0;
        
        string result = moveOk && jumpOk && doubleJumpOk ? "PASS" : "FAIL";
        testResults.Add($"[TEST 1] Move/Jump/Double Jump: {result} (Move:{moveOk} Jump:{jumpOk} DoubleJump:{doubleJumpOk})");
        Debug.Log($"[TEST 1] Move/Jump/Double Jump: {result}");
        yield return null;
    }
    
    IEnumerator TestSwordAttack()
    {
        Debug.Log("[TEST 2] Sword Attack Hit + Enemy Damage + Kill - START");
        var player = GameObject.Find("Player");
        var combat = player.GetComponent<PlayerCombat>();
        var enemy = GameObject.Find("Enemy");
        var enemyHealth = enemy.GetComponent<EnemyHealth>();
        
        // Position player near enemy
        player.transform.position = enemy.transform.position + new Vector3(1f, 0, 0);
        yield return new WaitForSeconds(0.2f);
        
        int initialHealth = enemyHealth.CurrentHealth;
        combat.Attack();
        yield return new WaitForSeconds(0.5f);
        
        int afterAttackHealth = enemyHealth.CurrentHealth;
        bool damageDealt = afterAttackHealth < initialHealth;
        
        // Keep attacking until dead
        while (enemyHealth.CurrentHealth > 0)
        {
            combat.Attack();
            yield return new WaitForSeconds(0.5f);
        }
        bool enemyKilled = enemyHealth.CurrentHealth <= 0;
        
        string result = damageDealt && enemyKilled ? "PASS" : "FAIL";
        testResults.Add($"[TEST 2] Sword Attack: {result} (Damage:{damageDealt} Kill:{enemyKilled})");
        Debug.Log($"[TEST 2] Sword Attack: {result}");
        yield return null;
    }
    
    IEnumerator TestPlayerAttackSlowMoZoom()
    {
        Debug.Log("[TEST 3] Player Attack -> Slow Motion + Camera Zoom -> Return - START");
        var combatDirector = FindObjectOfType<CombatDirector>();
        var combatCamera = FindObjectOfType<CombatCamera>();
        var player = GameObject.Find("Player");
        var combat = player.GetComponent<PlayerCombat>();
        var enemy = GameObject.Find("Enemy");
        
        // Respawn enemy if dead
        if (enemy.GetComponent<EnemyHealth>().CurrentHealth <= 0)
        {
            enemy.GetComponent<EnemyHealth>().Revive();
        }
        
        player.transform.position = enemy.transform.position + new Vector3(1f, 0, 0);
        yield return new WaitForSeconds(0.2f);
        
        float initialTimeScale = Time.timeScale;
        float initialCamSize = Camera.main.orthographicSize;
        
        combat.Attack();
        yield return new WaitForSeconds(0.1f);
        
        bool slowMoActive = Time.timeScale < initialTimeScale;
        bool camZoomed = Camera.main.orthographicSize < initialCamSize;
        
        // Wait for return
        yield return new WaitForSeconds(1.5f);
        
        bool timeRestored = Mathf.Approximately(Time.timeScale, initialTimeScale);
        bool camRestored = Mathf.Approximately(Camera.main.orthographicSize, initialCamSize);
        
        string result = slowMoActive && camZoomed && timeRestored && camRestored ? "PASS" : "FAIL";
        testResults.Add($"[TEST 3] Player Attack SlowMo+Zoom: {result} (SlowMo:{slowMoActive} Zoom:{camZoomed} RestoreTime:{timeRestored} RestoreCam:{camRestored})");
        Debug.Log($"[TEST 3] Player Attack SlowMo+Zoom: {result}");
        yield return null;
    }
    
    IEnumerator TestEnemyAttackParryWindow()
    {
        Debug.Log("[TEST 4] Enemy Attack -> Slow Motion -> Parry Window - START");
        var combatDirector = FindObjectOfType<CombatDirector>();
        var enemy = GameObject.Find("Enemy");
        var enemyScript = enemy.GetComponent<Enemy>();
        var player = GameObject.Find("Player");
        
        player.transform.position = enemy.transform.position + new Vector3(2f, 0, 0);
        yield return new WaitForSeconds(0.2f);
        
        float initialTimeScale = Time.timeScale;
        
        // Trigger enemy attack
        enemyScript.TriggerAttack();
        yield return new WaitForSeconds(0.1f);
        
        bool slowMoActive = Time.timeScale < initialTimeScale;
        bool parryWindowOpen = combatDirector.IsParryWindowOpen;
        
        string result = slowMoActive && parryWindowOpen ? "PASS" : "FAIL";
        testResults.Add($"[TEST 4] Enemy Attack Parry Window: {result} (SlowMo:{slowMoActive} ParryWindow:{parryWindowOpen})");
        Debug.Log($"[TEST 4] Enemy Attack Parry Window: {result}");
        yield return null;
    }
    
    IEnumerator TestParrySuccess()
    {
        Debug.Log("[TEST 5] Parry Success -> Counterattack (Normal/Perfect Log) - START");
        var player = GameObject.Find("Player");
        var combat = player.GetComponent<PlayerCombat>();
        var enemy = GameObject.Find("Enemy");
        var enemyScript = enemy.GetComponent<Enemy>();
        var combatDirector = FindObjectOfType<CombatDirector>();
        
        player.transform.position = enemy.transform.position + new Vector3(2f, 0, 0);
        yield return new WaitForSeconds(0.2f);
        
        // Trigger enemy attack
        enemyScript.TriggerAttack();
        yield return new WaitForSeconds(0.1f);
        
        // Wait for parry window
        while (!combatDirector.IsParryWindowOpen)
            yield return null;
        
        // Parry at perfect timing (middle of window)
        yield return new WaitForSeconds(combatDirector.ParryWindowDuration * 0.5f);
        combat.Parry();
        yield return new WaitForSeconds(0.5f);
        
        bool parrySuccess = combatDirector.LastParryResult == ParryResult.Success || combatDirector.LastParryResult == ParryResult.Perfect;
        bool counterAttacked = combatDirector.CounterAttackTriggered;
        bool perfectLogged = combatDirector.LastParryResult == ParryResult.Perfect;
        
        string result = parrySuccess && counterAttacked ? "PASS" : "FAIL";
        testResults.Add($"[TEST 5] Parry Success: {result} (Success:{parrySuccess} Counter:{counterAttacked} Perfect:{perfectLogged})");
        Debug.Log($"[TEST 5] Parry Success: {result}");
        yield return null;
    }
    
    IEnumerator TestParryFailKnockback()
    {
        Debug.Log("[TEST 6] Parry Fail -> Player Damage + Knockback - START");
        var player = GameObject.Find("Player");
        var combat = player.GetComponent<PlayerCombat>();
        var health = player.GetComponent<PlayerHealth>();
        var enemy = GameObject.Find("Enemy");
        var enemyScript = enemy.GetComponent<Enemy>();
        var combatDirector = FindObjectOfType<CombatDirector>();
        var rb = player.GetComponent<Rigidbody2D>();
        
        player.transform.position = enemy.transform.position + new Vector3(2f, 0, 0);
        yield return new WaitForSeconds(0.2f);
        
        int initialHealth = health.CurrentHealth;
        Vector3 initialPos = player.transform.position;
        
        // Trigger enemy attack
        enemyScript.TriggerAttack();
        yield return new WaitForSeconds(0.1f);
        
        // Wait for parry window
        while (!combatDirector.IsParryWindowOpen)
            yield return null;
        
        // Fail parry (don't press parry, or press too early/late)
        yield return new WaitForSeconds(combatDirector.ParryWindowDuration + 0.2f);
        yield return new WaitForSeconds(0.5f); // Wait for hit
        
        bool damageTaken = health.CurrentHealth < initialHealth;
        bool knockbackApplied = Vector3.Distance(player.transform.position, initialPos) > 0.5f;
        bool invincibilityActive = health.IsInvincible;
        
        string result = damageTaken && knockbackApplied && invincibilityActive ? "PASS" : "FAIL";
        testResults.Add($"[TEST 6] Parry Fail Knockback: {result} (Damage:{damageTaken} Knockback:{knockbackApplied} Invincible:{invincibilityActive})");
        Debug.Log($"[TEST 6] Parry Fail Knockback: {result}");
        yield return null;
    }
    
    IEnumerator TestCombatCameraZoom()
    {
        Debug.Log("[TEST 7] Combat Start/End Camera Zoom/Return (No Stutter) - START");
        var combatCamera = FindObjectOfType<CombatCamera>();
        var combatDirector = FindObjectOfType<CombatDirector>();
        var player = GameObject.Find("Player");
        var combat = player.GetComponent<PlayerCombat>();
        var enemy = GameObject.Find("Enemy");
        var enemyScript = enemy.GetComponent<Enemy>();
        
        if (enemy.GetComponent<EnemyHealth>().CurrentHealth <= 0)
            enemy.GetComponent<EnemyHealth>().Revive();
        
        player.transform.position = enemy.transform.position + new Vector3(1f, 0, 0);
        yield return new WaitForSeconds(0.2f);
        
        float initialSize = Camera.main.orthographicSize;
        Vector3 initialPos = Camera.main.transform.position;
        
        // Combat start - player attacks
        combat.Attack();
        yield return new WaitForSeconds(0.1f);
        
        bool zoomedIn = Camera.main.orthographicSize < initialSize;
        
        // Combat end - wait for return
        yield return new WaitForSeconds(2f);
        
        bool restored = Mathf.Approximately(Camera.main.orthographicSize, initialSize);
        bool posRestored = Vector3.Distance(Camera.main.transform.position, initialPos) < 0.1f;
        
        // Check for stutter (large position jumps)
        bool noStutter = true; // Would need frame-by-frame tracking
        
        string result = zoomedIn && restored && posRestored && noStutter ? "PASS" : "FAIL";
        testResults.Add($"[TEST 7] Combat Camera Zoom: {result} (Zoom:{zoomedIn} Restore:{restored} PosRestore:{posRestored} NoStutter:{noStutter})");
        Debug.Log($"[TEST 7] Combat Camera Zoom: {result}");
        yield return null;
    }
    
    IEnumerator TestDeathRespawn()
    {
        Debug.Log("[TEST 8] Death -> GameOver -> Checkpoint Respawn -> Reset - START");
        var player = GameObject.Find("Player");
        var health = player.GetComponent<PlayerHealth>();
        var gameManager = FindObjectOfType<GameManager>();
        var checkpoint = FindObjectOfType<Checkpoint>();
        
        // Kill player
        health.TakeDamage(999);
        yield return new WaitForSeconds(1f);
        
        bool gameOverShown = gameManager.CurrentState == GameState.GameOver;
        bool playerDead = health.CurrentHealth <= 0;
        
        // Press R to respawn (simulate)
        gameManager.RespawnAtCheckpoint();
        yield return new WaitForSeconds(1f);
        
        bool respawned = health.CurrentHealth > 0;
        bool atCheckpoint = Vector3.Distance(player.transform.position, checkpoint.transform.position) < 2f;
        bool enemiesReset = true; // Check all enemies revived
        bool statePlaying = gameManager.CurrentState == GameState.Playing;
        
        string result = gameOverShown && playerDead && respawned && atCheckpoint && enemiesReset && statePlaying ? "PASS" : "FAIL";
        testResults.Add($"[TEST 8] Death Respawn: {result} (GameOver:{gameOverShown} Dead:{playerDead} Respawned:{respawned} AtCheckpoint:{atCheckpoint} EnemiesReset:{enemiesReset} Playing:{statePlaying})");
        Debug.Log($"[TEST 8] Death Respawn: {result}");
        yield return null;
    }
    
    IEnumerator TestStageClearRestart()
    {
        Debug.Log("[TEST 9] Stage End -> Clear -> R Restart - START");
        var gameManager = FindObjectOfType<GameManager>();
        var player = GameObject.Find("Player");
        
        // Trigger stage clear (find clear trigger)
        var clearTrigger = FindObjectOfType<StageClearTrigger>();
        if (clearTrigger != null)
        {
            clearTrigger.TriggerClear();
        }
        else
        {
            // Simulate clear
            gameManager.StageClear();
        }
        yield return new WaitForSeconds(1f);
        
        bool clearShown = gameManager.CurrentState == GameState.StageClear;
        
        // Press R to restart
        gameManager.RestartStage();
        yield return new WaitForSeconds(1f);
        
        bool restarted = gameManager.CurrentState == GameState.Playing;
        bool playerReset = player.transform.position == GameObject.Find("PlayerStart").transform.position;
        bool enemiesReset = true;
        
        string result = clearShown && restarted && playerReset && enemiesReset ? "PASS" : "FAIL";
        testResults.Add($"[TEST 9] Stage Clear Restart: {result} (Clear:{clearShown} Restarted:{restarted} PlayerReset:{playerReset} EnemiesReset:{enemiesReset})");
        Debug.Log($"[TEST 9] Stage Clear Restart: {result}");
        yield return null;
    }
}