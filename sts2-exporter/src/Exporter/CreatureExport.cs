using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace STS2Export.Exporter;

public class CreatureExport : ItemExport, IImageExport {
    public CreatureExport(MonsterModel monsterModel) {
        Assembly = monsterModel.GetType().Assembly;
        ID = monsterModel.Id.Entry;
        name = monsterModel.Title.GetFormattedText();
        minHP = monsterModel.MinInitialHp;
        maxHP = monsterModel.MaxInitialHp;
        this.monsterModel = monsterModel.ToMutable();
    }

    public CreatureExport(CharacterModel characterModel) {
        Assembly = characterModel.GetType().Assembly;
        ID = characterModel.Id.Entry;
        name = characterModel.Title.GetFormattedText();
        type = "Player";
        //minHP = characterModel.StartingHp;
        maxHP = characterModel.StartingHp;
        this.characterModel = (CharacterModel)characterModel.MutableClone();
    }

    private readonly MonsterModel monsterModel;
    private readonly CharacterModel characterModel;

    [JsonInclude][JsonPropertyName("id")]
    private readonly string ID;
    [JsonInclude][JsonPropertyName("name")]
    private readonly string name;
    [JsonInclude][JsonPropertyName("type")]
    private readonly string type = "";
    [JsonInclude][JsonPropertyName("minHP")]
    private readonly int minHP;
    [JsonInclude][JsonPropertyName("maxHP")]
    private readonly int maxHP;
    [JsonInclude][JsonPropertyName("minHPA")]
    private readonly int minHPAscension;
    [JsonInclude][JsonPropertyName("maxHPA")]
    private readonly int maxHPAscension;

    public static List<CreatureExport> FindAll() => [
        ..ModelDb.AllCharacters.Select(m => new CreatureExport(m)),
        ..ModelDb.Monsters.OrderBy(m => m.Id.Entry).Select(m => new CreatureExport(m)),
    ];

    public ViewportManager.DrawRequest[] ExportImg(ExportConfig config) {
        NCreatureVisuals visuals = null;
        if (monsterModel != null)
            visuals = monsterModel.CreateVisuals();
        else if (characterModel != null)
            visuals = characterModel.CreateVisuals();
        Vector2I bounds = visuals.HasSpineAnimation ? (Vector2I)visuals.SpineBody.GetSkeleton().GetBounds().Size : (Vector2I)visuals.GetNode<Control>("%Bounds").Size;
        return [new(bounds, $"creatures/{ID}", null, drawer => {
            drawer.AddChild(visuals);
            if (visuals != null && visuals.HasSpineAnimation) {
                var animController = visuals.SpineBody;
                var animState = visuals.SpineBody.GetAnimationState();
                animState.SetTimeScale(0f);
                visuals.SpineBody.GetSkeleton().GetBounds();
                if (monsterModel != null) {
                    monsterModel.GenerateAnimator(animController);
                    visuals.SetUpSkin(monsterModel);
                }
                else if (characterModel != null) {
                    //characterModel.Creature = new(characterModel, MegaCrit.Sts2.Core.Combat.CombatSide.Enemy, null);
                    characterModel.GenerateAnimator(animController);
                }
                animState.SetAnimation("idle_loop");
            }
            visuals.Position = bounds / 2 + new Vector2(0f, bounds.Y * 0.5f);
            visuals.Show();
            visuals.Modulate = Colors.White;
        }, waitExtraFrames: 1)];
    }
}