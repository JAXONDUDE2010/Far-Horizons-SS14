using Content.Client.Lobby;
using Content.Server.Preferences.Managers;
using Content.Shared.Preferences;
using Robust.Client.State;
using Robust.Shared.Network;

namespace Content.IntegrationTests.Tests.Lobby;

//Starlight, I diverged this file since our character profile system is different. Not a fan but whatever

[TestFixture]
[TestOf(typeof(ClientPreferencesManager))]
[TestOf(typeof(ServerPreferencesManager))]
public sealed class CharacterCreationTest
{
    [Test]
    public async Task CreateDeleteCreateTest()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { InLobby = true });
        var server = pair.Server;
        var client = pair.Client;

        var clientNetManager = client.ResolveDependency<IClientNetManager>();
        var clientStateManager = client.ResolveDependency<IStateManager>();
        var clientPrefManager = client.ResolveDependency<IClientPreferencesManager>();

        var serverPrefManager = server.ResolveDependency<IServerPreferencesManager>();


        // Need to run them in sync to receive the messages.
        await pair.RunTicksSync(1);

        await PoolManager.WaitUntil(client, () => clientStateManager.CurrentState is LobbyState, 600);

        Assert.That(clientNetManager.ServerChannel, Is.Not.Null);

        var clientNetId = clientNetManager.ServerChannel.UserId;
        HumanoidCharacterProfile profile = null;

        await client.WaitAssertion(() =>
        {
            var clientCharacters = clientPrefManager.Preferences?.Characters;
            Assert.That(clientCharacters, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(clientCharacters, Has.Count.EqualTo(1));

                Assert.That(clientStateManager.CurrentState, Is.TypeOf<LobbyState>());
            });

            profile = HumanoidCharacterProfile.Random();
            clientPrefManager.CreateCharacter(profile);

            clientCharacters = clientPrefManager.Preferences?.Characters;

            Assert.That(clientCharacters, Is.Not.Null);
            Assert.That(clientCharacters, Has.Count.EqualTo(2));
            Assert.That(clientCharacters[1].MemberwiseEquals(profile));
        });

        await PoolManager.WaitUntil(server, () => serverPrefManager.GetPreferences(clientNetId).Characters.Count == 2, maxTicks: 60);

        await server.WaitAssertion(() =>
        {
            var serverCharacters = serverPrefManager.GetPreferences(clientNetId).Characters;

            Assert.That(serverCharacters, Has.Count.EqualTo(2));
            Assert.That(serverCharacters[1].MemberwiseEquals(profile));
        });

        await client.WaitAssertion(() =>
        {
            clientPrefManager.DeleteCharacter(1);

            var clientCharacters = clientPrefManager.Preferences?.Characters.Count;
            Assert.That(clientCharacters, Is.EqualTo(1));
        });

        await PoolManager.WaitUntil(server, () => serverPrefManager.GetPreferences(clientNetId).Characters.Count == 1, maxTicks: 60);

        await server.WaitAssertion(() =>
        {
            var serverCharacters = serverPrefManager.GetPreferences(clientNetId).Characters.Count;
            Assert.That(serverCharacters, Is.EqualTo(1));
        });

        await client.WaitIdleAsync();

        await client.WaitAssertion(() =>
        {
            profile = HumanoidCharacterProfile.Random();

            clientPrefManager.CreateCharacter(profile);

            var clientCharacters = clientPrefManager.Preferences?.Characters;

            Assert.That(clientCharacters, Is.Not.Null);
            Assert.That(clientCharacters, Has.Count.EqualTo(2));
            Assert.That(clientCharacters[1].MemberwiseEquals(profile));
        });

        await PoolManager.WaitUntil(server, () => serverPrefManager.GetPreferences(clientNetId).Characters.Count == 2, maxTicks: 60);

        await server.WaitAssertion(() =>
        {
            var serverCharacters = serverPrefManager.GetPreferences(clientNetId).Characters;

            Assert.That(serverCharacters, Has.Count.EqualTo(2));
            Assert.That(serverCharacters[1].MemberwiseEquals(profile));
        });
        await pair.CleanReturnAsync();
    }

    private void AssertEqual(HumanoidCharacterProfile a, HumanoidCharacterProfile b)
    {
        if (a.MemberwiseEquals(b))
            return;

        Assert.Multiple(() =>
        {
            Assert.That(a.Name, Is.EqualTo(b.Name));
            Assert.That(a.Age, Is.EqualTo(b.Age));
            Assert.That(a.Sex, Is.EqualTo(b.Sex));
            Assert.That(a.Gender, Is.EqualTo(b.Gender));
            Assert.That(a.Species, Is.EqualTo(b.Species));
            Assert.That(a.PreferenceUnavailable, Is.EqualTo(b.PreferenceUnavailable));
            Assert.That(a.SpawnPriority, Is.EqualTo(b.SpawnPriority));
            Assert.That(a.FlavorText, Is.EqualTo(b.FlavorText));
            Assert.That(a.JobPriorities, Is.EquivalentTo(b.JobPriorities));
            Assert.That(a.AntagPreferences, Is.EquivalentTo(b.AntagPreferences));
            Assert.That(a.TraitPreferences, Is.EquivalentTo(b.TraitPreferences));
            Assert.That(a.Loadouts, Is.EquivalentTo(b.Loadouts));
            AssertEqual(a.Appearance, b.Appearance);
            Assert.Fail("Profile not equal");
        });
    }

    private void AssertEqual(HumanoidCharacterAppearance a, HumanoidCharacterAppearance b)
    {
        if (a.Equals(b))
            return;

        Assert.That(a.EyeColor, Is.EqualTo(b.EyeColor));
        Assert.That(a.SkinColor, Is.EqualTo(b.SkinColor));
        Assert.That(a.Markings, Is.EquivalentTo(b.Markings));
        Assert.Fail("Appearance not equal");
    }
}
