# DesenhoNET
Projeto conceitual para estudo do menssageiro RabbitMQ.
Após estudo em seu site oficial descidi misturar vários conceitos e colocar em pratica.

Nesse projeto é possível criar uma sala para desenhar, então uma id da sala será criada para ser compartilhada.
Com o id é possivel que outras pessoas se conectem e desenhem junto com você, em tempo real.

Tecnologias usadas:
 - .Net Core MVC
 - Javascript
 - Docker
 - WebSockets
 - RabbitMQ

Segue o desenho do esquema de comunicação, ao clicar na imagem será direcionado ao um vídeo com exemplo de funcionamento no youtube:

[![Esquema de comunicação](https://dsm01pap002files.storage.live.com/y4m4fD4ohb0zTXFV819NsgUOWUBcKiNDSVRTvMJ9Ule_fARMS8ltra6LMCVrx9T3ATHTistjNibVB1QLOz4AC6lh9YSaZeRIr77fBB9s_Yb2xg1bGG6AmC_hx2h3PVmu12h249say79R4XPLXkx5Yr0oWzh8oGx_TRh54Qe0bDCzXR_ZnADZt9kflaP6smwnOpA?width=1123&height=794&cropmode=none)](https://youtu.be/N7CpGKJpJ38)

# Teste em seu computador

*Atenção, para usar esse projeto será necessário Docker e Docker-Composer intalado e configurado em sua máquina.
O projeto usa as portas 5000 para aplicação .NET e 8080 para o painel RabbitMQ, ambos podem ser alterados em docker-compose.debug.yml*

1- Clone o repositório.

2- Rode o código na pasta raiz do projeto atravez de um prompt de comando
```sh
docker-compose -f docker-compose.debug.yml up --detach --build
```
Ou rode a task RunOps no Visual Studio Code.

3- Aguarde o build da imagem e do projeto e dopois é só testar no endereço
http://[ipVinculadoAoDocker]:5000

