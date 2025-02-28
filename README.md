# UtilityBasedAI---Unity
Este projeto é um estudo que visa entender, de maneira simplificada, como a lógica Utility Based funciona em jogos como the Sims.
O Projeto foi desenvolvido na versão 2021.03 da Unity, então ele pode apresentar erros em outras versões.


* Se quiser utilizar os scripts em um projeto a parte ou entender como funciona, siga os paços:

  1. Crie um Objeto "GameController" a atribua o script "gameController.cs" nele.
  2. Instale o NavMeshPlus-Master, crie o cenário que deseja e de bake.
  3. crie um personagem e atribua a ele um "navMesh agent", o script "nav.cs"(responsavel pelas animações do sprite e de executar o navMesh) e o "simController.cs"(onde será executada a lógica UtilityBased dele).
  4. Configure o simController, adicionando todas as "necessidades" que ele pode ter, suas curvas de urgencia e sliders para ser mostrado na UI suas necessidades. Edite o script como for adequado para você, para adicionar mais necessidades padroes abra o script "needs.cs".
  5. Adicione obejtos na cena e coloque neles o script "Object.cs", configure para o que o objeto serve e quantos pontos ele da por segundo para o personagem.

