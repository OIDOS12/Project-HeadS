using UnityEngine;
using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

public class MovementTest
{
    private GameObject playerGO;
    private OfflinePlayerMovementController controller;
    private Rigidbody2D rb;
    private BoxCollider2D box;
    private GameObject playerModel;
    private System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

    [SetUp]
    public void SetUp()
    {
        playerGO = new GameObject("Player");
        controller = playerGO.AddComponent<OfflinePlayerMovementController>();
        rb = playerGO.AddComponent<Rigidbody2D>();
        box = playerGO.AddComponent<BoxCollider2D>();
        playerModel = new GameObject("PlayerModel");
        playerModel.SetActive(true);

        controller.GetType().GetField("rb", flags).SetValue(controller, rb);
        controller.GetType().GetField("boxCollider", flags).SetValue(controller, box);
        controller.GetType().GetField("playerModel", flags).SetValue(controller, playerModel);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(playerGO);
        Object.DestroyImmediate(playerModel);
    }

    [UnityTest]
    public IEnumerator PlayerMovesRight_WhenApplyMovementCalled()
    {
        controller.GetType().GetField("speed", flags).SetValue(controller, 5f);

        var method = controller.GetType().GetMethod("ApplyMovement", flags);
        method.Invoke(controller, new object[] { 1f, false, false, true });

        Debug.Log("Moves right: rb.linearVelocity.x: " + rb.linearVelocity.x + ", expected: 5");
        Assert.AreEqual(5f, rb.linearVelocity.x, 0.001f);

        yield break;
    }

    [UnityTest]
    public IEnumerator PlayerMovesLeft_WhenApplyMovementCalled()
    {
        controller.GetType().GetField("speed", flags).SetValue(controller, 5f);

        var method = controller.GetType().GetMethod("ApplyMovement", flags);
        method.Invoke(controller, new object[] { -1f, false, false, true });

        Debug.Log("Moves left: rb.linearVelocity.x: " + rb.linearVelocity.x + ", expected: -5");
        Assert.AreEqual(-5f, rb.linearVelocity.x, 0.001f);

        yield break;
    }

    [UnityTest]
    public IEnumerator PlayerJumps_WhenJumpIsTrue()
    {
        controller.GetType().GetField("jumpForce", flags).SetValue(controller, 10f);

        var method = controller.GetType().GetMethod("ApplyMovement", flags);
        method.Invoke(controller, new object[] { 0f, true, false, true });

        Debug.Log("Jump true: rb.linearVelocity.y: " + rb.linearVelocity.y + ", expected: > 0");
        Assert.Greater(rb.linearVelocity.y, 0f);

        yield break;
    }

    [UnityTest]
    public IEnumerator PlayerDoesNotJump_WhenJumpIsFalse()
    {
        controller.GetType().GetField("jumpForce", flags).SetValue(controller, 10f);

        var method = controller.GetType().GetMethod("ApplyMovement", flags);
        method.Invoke(controller, new object[] { 0f, false, false, true });

        Debug.Log("Jump false: rb.linearVelocity.y: " + rb.linearVelocity.y + ", expected: 0");
        Assert.AreEqual(0f, rb.linearVelocity.y, 0.001f);

        yield break;
    }

}

