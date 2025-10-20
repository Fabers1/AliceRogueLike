using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryDefeatManager : MonoBehaviour
{
    [Header("Painéis")]
    public GameObject painelVitoria;
    public GameObject painelDerrota;

    [Header("Configurações de Cena")]
    public string nomeCenaMenu = "Menu"; // Nome da cena do menu principal

    void Start()
    {
        // Garante que os painéis comecem desativados
        if (painelVitoria != null)
            painelVitoria.SetActive(false);

        if (painelDerrota != null)
            painelDerrota.SetActive(false);
    }

    // Método para ativar painel de vitória
    public void MostrarVitoria()
    {
        if (painelVitoria != null)
        {
            painelVitoria.SetActive(true);
            Time.timeScale = 0f; // Pausa o jogo
        }
    }

    // Método para ativar painel de derrota
    public void MostrarDerrota()
    {
        if (painelDerrota != null)
        {
            painelDerrota.SetActive(true);
            Time.timeScale = 0f; // Pausa o jogo
        }
    }

    // Botão "Rejogar" no painel de derrota - Reinicia a fase do início
    public void RejogarFase()
    {
        Time.timeScale = 1f; // Retoma o tempo do jogo
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Botão "Menu" - Volta para o menu principal
    public void VoltarParaMenu()
    {
        Time.timeScale = 1f; // Retoma o tempo do jogo
        SceneManager.LoadScene(nomeCenaMenu);
    }



    // Método para sair do jogo (útil para botões de sair)
    public void SairDoJogo()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}