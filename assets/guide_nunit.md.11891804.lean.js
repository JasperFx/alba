import{o as n,c as s,a,b as t,e as p,d as o}from"./app.144e7b1a.js";const e='{"title":"","description":"","frontmatter":"title:Integrating with NUnit editLink:true","relativePath":"guide/nunit.md","lastUpdated":1629228268006}',c={},l=t("p",null,[p("When using Alba within "),t("a",{href:"./.html"},"NUnit testing projects"),p(", you probably want to reuse the "),t("code",null,"IAlbaHost"),p(" across tests and test fixtures because "),t("code",null,"AlbaHost"),p(" is relatively expensive to create (it's bootstrapping your whole application more than Alba itself is slow). To do that with NUnit, you could track a single "),t("code",null,"AlbaHost"),p(" on a static class like this one:")],-1),u=o('',4),i=o('',3);c.render=function(t,p,o,e,c,k){return n(),s("div",null,[l,a(" snippet: sample_NUnit_Application "),u,a(" snippet: sample_NUnit_scenario_test "),i])};export{e as __pageData,c as default};
