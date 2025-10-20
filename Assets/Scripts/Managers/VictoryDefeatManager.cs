using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryDefeatManager : MonoBehaviour
{
    [Header("Pain�is")]
    public GameObject painelVitoria;
    public GameObject painelDerrota;

    [Header("Configura��es de Cena")]
    public string nomeCenaMenu = "Menu"; // Nome da cena do menu principal

    void Start()
    {
        // Garante que os pain�is comecem desativados
        if (painelVitoria != null)
            painelVitoria.SetActive(false);

        if (painelDerrota != null)
            painelDerrota.SetActive(false);
    }

    // M�todo para ativar painel de vit�ria
    public void MostrarVitoria()
    {
        if (painelVitoria != null)
        {
            painelVitoria.SetActive(true);
            Time.timeScale = 0f; // Pausa o jogo
        }
    }

    // M�todo para ativar painel de derrota
    public void MostrarDerrota()
    {
        if (painelDerrota != null)
        {
            painelDerrota.SetActive(true);
            Time.timeScale = 0f; // Pausa o jogo
        }
    }

    // Bot�o "Rejogar" no painel de derrota - Reinicia a fase do in�cio
    public void RejogarFase()
    {
        Time.timeScale = 1f; // Retoma o tempo do jogo
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Bot�o "Menu" - Volta para o menu principal
    public void VoltarParaMenu()
    {
        Time.timeScale = 1f; // Retoma o tempo do jogo
        SceneManager.LoadScene(nomeCenaMenu);
    }



    // M�todo para sair do jogo (�til para bot�es de sair)
    public void SairDoJogo()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}