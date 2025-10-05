using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    // Dicion�rio para guardar a localiza��o dos itens -> <ID_do_Item, Nome_da_Cena>
    private Dictionary<string, string> itemSceneDatabase = new Dictionary<string, string>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Chamado por um item quando ele � criado para registrar sua localiza��o inicial
    public void RegisterItem(string itemId, string sceneName)
    {
        // S� registra se o item ainda n�o estiver no banco de dados
        if (!itemSceneDatabase.ContainsKey(itemId))
        {
            itemSceneDatabase[itemId] = sceneName;
            Debug.Log($"Item '{itemId}' registrado na cena '{sceneName}'.");
        }
    }

    // Chamado pelo Player ao soltar um item em uma nova cena
    public void UpdateItemScene(string itemId, string newSceneName)
    {
        if (itemSceneDatabase.ContainsKey(itemId))
        {
            itemSceneDatabase[itemId] = newSceneName;
            Debug.Log($"Item '{itemId}' teve sua localiza��o atualizada para a cena '{newSceneName}'.");
        }
    }

    // Chamado por um item ao carregar para verificar onde ele deveria estar
    public string GetItemCurrentScene(string itemId)
    {
        if (itemSceneDatabase.ContainsKey(itemId))
        {
            return itemSceneDatabase[itemId];
        }
        return null; // Retorna null se o item n�o for encontrado
    }
}