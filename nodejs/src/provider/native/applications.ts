import type { CsharpTargetBindingFact } from "@tsonic/target-csharp/provider";
import { dotnetTypeRefToTargetTypeRef } from "@tsonic/target-csharp/provider/dotnet";
import type { NodejsNativeClassApplicationRecipe } from "./locators.js";
import type { NodejsNativeResolution, NodejsNativeSelection } from "./selection.js";
import { rejected, resolved } from "./selection.js";

export function nodejsNativeClassApplication(
  selection: NodejsNativeSelection,
  recipe: NodejsNativeClassApplicationRecipe,
): NodejsNativeResolution<CsharpTargetBindingFact> {
  const declaration = selection.declaration(recipe.original);
  if (declaration.kind === "rejected") return declaration;
  if (recipe.kind === "source-class") return resolved(declaration.value.binding);
  const selected = selection.member(recipe.selected);
  if (selected.kind === "rejected") return selected;
  const parameter = selected.value.signature?.parameters[recipe.targetParameterIndex];
  if (parameter?.type.kind !== "nullable-reference" ||
    parameter.type.elementType.kind !== "named" ||
    parameter.type.elementType.targetId !== declaration.value.original.targetId ||
    parameter.type.elementType.typeArguments?.length !== recipe.original.typeParameterCount) {
    return rejected("The authored Node class application requires the exact selected Native parameter and reference annotation.", recipe.original.metadataName);
  }
  const csharpType = dotnetTypeRefToTargetTypeRef(parameter.type.elementType);
  if (csharpType.kind !== "target-named" || csharpType.id !== declaration.value.binding.id) {
    return rejected("The acquired closed Node application must retain its canonical open Native declaration identity.", recipe.original.metadataName);
  }
  return resolved(Object.freeze({ ...declaration.value.binding, csharpType }));
}
