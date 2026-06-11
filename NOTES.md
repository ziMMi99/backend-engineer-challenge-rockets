# Interview Notes

## Tradeoffs

- Jeg valgte `InMemory` i stedet for en database, fordi tiden var begrænset.
- Jeg lavede custom unit tests i stedet for at bruge et testframework, også fordi tiden var begrænset.
- En maintainability tradeoff er, at `InMemory` storage betyder, at alt data forsvinder når appen restarter. Men det er en pragmatisk løsning til opgaven, og jeg ville kalde det en bevidst simplificering.

## AI brug

- Jeg lavede en plan for udviklingen af opgaven.
- Jeg brugte AI til sparring på arkitektur og kode.
- Jeg brugte AI til at sætte scaffolding op, hjælpe med formattering, skrive forklaringer og lave `README` / `spec` filer.
- Jeg brugte det primært som hjælp til at tænke og lære om:
  - envelope vs typed payloads
  - in-memory store vs database
  - handler vs projector vs aggregate ansvar
- Jeg verificerede tricky logic igennem tests og ved at læse implementationen igennem og forstå hvert skridt i koden.
- Når AI foreslog en løsning, der blev for stor eller for bred, sørgede jeg for at gøre løsningen smallere, så den passede til projektets tidsplan og scope.

## Hvad virkede godt med AI

- Det var nemt og hurtigt at sætte et projekt op fra bunden.
- Det var nemt og hurtigt at få overblik over, hvad projektet krævede, og hvad løsningen skulle have.

## Hvad var risikabelt

- AI foreslog en løsning på duplicate og out-of-order beskeder, som bare ignorerede dem i stedet for at håndtere dem korrekt, og det ville ikke passe med målet for opgaven. Her var jeg opmærksom og skrev, at det ikke var den rigtige løsning.
- At lave en custom test setup i stedet for at bruge et framework kan være en risiko, men det var en pragmatisk løsning, fordi det virkede uden at bringe ekstra frameworks og dependencies ind i koden. Det holder projektet smalt og læsbart.

## Hvad ville jeg gøre anderledes i et produktion system?

- Bruge en database i stedet for `InMemory`.
- Bruge rigtige HTTP integration tests / unit tests med et testframework.
- Lave stærkere validation på inputtet, så malformed beskeder og lignende bliver håndteret bedre.
- Tilføje observability som logging, metrics og tracing i message-processen.

## Design choices / implementation notes

- Jeg valgte en `InMemory` løsning med en per-rocket aggregate, fordi målet med løsningen ikke var persistens, men message ordering.
- Jeg håndterede en rocket som en stream af beskeder i stedet for som et state object, hvilket gjorde out-of-order og duplicate-håndtering meget nemmere.
- Jeg fordelte den kritiske forretningslogik ud i egne filer for at holde koden vedligeholdelig og testbar. Det hjælper også på læsbarheden og den generelle forståelse af projektet.
- Jeg brugte låsning i aggregaten, så kun én tråd kan arbejde med en rocket stream ad gangen, for at håndtere mulige race conditions.
- Jeg sorterede rockets baseret på ID, når man henter dem alle, fordi opgaven ikke beskrev noget andet.
- Koden er splittet i mindre dele for at opretholde læsbarhed og vedligeholdelse.
- Projector-patternet holder koden nem at teste.
- Aggregaten håndterer de tricky ting som out-of-order, terminal state handling osv.
- Implementationen er med vilje tynd og ikke framework-heavy for at opretholde læsbarhed.
- Der er lavet tests for de vigtigste funktioner såsom out-of-order, duplicate delivery, late redelivery og messages arriving before launch.
- Der er sat HTTP tests op, så endpoints og applikationens funktionalitet kan testes direkte.
- Pga. tidspres brugte jeg agentic AI til at udvikle stream-aggregaten. For at sikre mig, at jeg kunne forsvare koden, bad jeg den begrænse scope til kun at håndtere opgaven og ikke gøre det mere kompliceret end nødvendigt. Efter det brugte jeg tid på at forstå det og sikrede mig gennem test, at jeg faktisk forstod det.

