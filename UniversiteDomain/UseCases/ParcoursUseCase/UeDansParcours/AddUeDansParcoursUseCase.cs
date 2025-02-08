using UniversiteDomain.DataAdapters;
using UniversiteDomain.Entities;
using UniversiteDomain.Exceptions.ParcoursExceptions;
using UniversiteDomain.Exceptions.UeExceptions;

namespace UniversiteDomain.UseCases.ParcoursUseCases.UeDansParcours;

public class AddUeDansParcoursUseCase(IRepositoryFactory repositoryFactory)
{ 
    public async Task<Parcours> ExecuteAsync(long idParcours, long idUe)
    {
        Console.WriteLine($"Début d'ExecuteAsync avec idParcours={idParcours}, idUe={idUe}");

        await CheckBusinessRules(idParcours, idUe);

        Console.WriteLine("Business rules validées. Ajout de l'UE en cours...");

        var result = await repositoryFactory.ParcoursRepository().AddUeAsync(idParcours, idUe);

        Console.WriteLine("UE ajoutée avec succès !");
        return result;
    }

    private async Task CheckBusinessRules(long idParcours, long idUe)
    {
        Console.WriteLine($"Vérification des règles métier pour idParcours={idParcours}, idUe={idUe}");

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(idParcours);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(idUe);

        ArgumentNullException.ThrowIfNull(repositoryFactory);
        ArgumentNullException.ThrowIfNull(repositoryFactory.UeRepository());
        ArgumentNullException.ThrowIfNull(repositoryFactory.ParcoursRepository());

        // Vérification de l'existence de l'UE
        List<Ue> ue = await repositoryFactory.UeRepository().FindByConditionAsync(e => e.Id.Equals(idUe));
        if (ue.Count == 0) throw new UeNotFoundException($"UE avec ID {idUe} non trouvée.");

        // Vérification de l'existence du parcours
        List<Parcours> parcours = await repositoryFactory.ParcoursRepository().FindByConditionAsync(p => p.Id.Equals(idParcours));
        if (parcours.Count == 0) throw new ParcoursNotFoundException($"Parcours avec ID {idParcours} non trouvé.");

        // Vérification si l'UE est déjà enregistrée
        if (parcours[0].UesEnseignees != null)
        {
            List<Ue> inscrites = parcours[0].UesEnseignees;
            var trouve = inscrites.Find(e => e.Id.Equals(idUe));
            if (trouve != null)
                throw new DuplicateUeDansParcoursException($"UE {idUe} est déjà présente dans le parcours {idParcours}.");
        }
    }
}
