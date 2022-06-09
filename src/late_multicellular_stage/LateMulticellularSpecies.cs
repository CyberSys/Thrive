﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;

/// <summary>
///   Represents a late multicellular species that is 3D and composed of placed tissues
/// </summary>
[JsonObject(IsReference = true)]
[TypeConverter(typeof(ThriveTypeConverter))]
[JSONDynamicTypeAllowed]
[UseThriveConverter]
public class LateMulticellularSpecies : Species
{
    public LateMulticellularSpecies(uint id, string genus, string epithet) : base(id, genus, epithet)
    {
    }

    [JsonProperty]
    public MulticellularMetaballLayout BodyLayout { get; private set; } = new();

    [JsonProperty]
    public List<CellType> CellTypes { get; private set; } = new();

    /// <summary>
    ///   The scale in meters of the species
    /// </summary>
    public float Scale { get; set; } = 1.0f;

    /// <summary>
    ///   All organelles in all of the species' placed metaballs (there can be a lot of duplicates in this list)
    /// </summary>
    [JsonIgnore]
    public IEnumerable<OrganelleTemplate> Organelles =>
        BodyLayout.Select(m => m.CellType).Distinct().SelectMany(c => c.Organelles);

    [JsonIgnore]
    public override string StringCode => ThriveJsonConverter.Instance.SerializeObject(this);

    public override void OnEdited()
    {
        RepositionToOrigin();
        UpdateInitialCompounds();
    }

    public override void RepositionToOrigin()
    {
        throw new NotImplementedException();

        // BodyLayout.RepositionToOrigin();
    }

    public override void UpdateInitialCompounds()
    {
        var simulation = SimulationParameters.Instance;

        var rusticyanin = simulation.GetOrganelleType("rusticyanin");
        var chemo = simulation.GetOrganelleType("chemoplast");
        var chemoProtein = simulation.GetOrganelleType("chemoSynthesizingProteins");

        if (Organelles.Any(o => o.Definition == rusticyanin))
        {
            SetInitialCompoundsForIron();
        }
        else if (Organelles.Any(o => o.Definition == chemo ||
                     o.Definition == chemoProtein))
        {
            SetInitialCompoundsForChemo();
        }
        else
        {
            SetInitialCompoundsForDefault();
        }
    }

    public override void ApplyMutation(Species mutation)
    {
        base.ApplyMutation(mutation);

        var casted = (EarlyMulticellularSpecies)mutation;

        // TODO: layout
        BodyLayout.Clear();
        throw new NotImplementedException();

        CellTypes.Clear();

        foreach (var cellType in casted.CellTypes)
        {
            CellTypes.Add((CellType)cellType.Clone());
        }
    }

    public override object Clone()
    {
        var result = new LateMulticellularSpecies(ID, Genus, Epithet);

        ClonePropertiesTo(result);

        foreach (var cellType in CellTypes)
        {
            result.CellTypes.Add((CellType)cellType.Clone());
        }

        foreach (var metaball in BodyLayout)
        {
            result.BodyLayout.Add((MulticellularMetaball)metaball.Clone());
        }

        return result;
    }

    private void SetInitialCompoundsForDefault()
    {
        InitialCompounds.Clear();

        // TODO: modify these numbers based on the scale and metaball count or something more accurate
        InitialCompounds.Add(SimulationParameters.Instance.GetCompound("atp"), 180);
        InitialCompounds.Add(SimulationParameters.Instance.GetCompound("glucose"), 90);
    }

    private void SetInitialCompoundsForIron()
    {
        SetInitialCompoundsForDefault();
        InitialCompounds.Add(SimulationParameters.Instance.GetCompound("iron"), 90);
    }

    private void SetInitialCompoundsForChemo()
    {
        SetInitialCompoundsForDefault();
        InitialCompounds.Add(SimulationParameters.Instance.GetCompound("hydrogensulfide"), 90);
    }
}