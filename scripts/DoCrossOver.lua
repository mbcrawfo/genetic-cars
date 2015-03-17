-- Performs a crossover by selecting a random starting parent and copying bits
-- from that parent with a chance to flip parents after each bit is copied.
-- param genomeA: The first parent genome string.
-- param genomeB: The second parent genome string.
-- param flipChance: The percent chance (between 0 and 1) to flip parents.
-- returns: The newly created genome string.
function DoCrossOver(genomeA, genomeB, flipChance)
  local parent
  local child = ""
  
  if RNG:NextDouble() < 0.5 then
    parent = genomeA
  else
    parent = genomeB
  end
  
  for i = 1, GenomeLength do
    child = child .. parent:sub(i, i)
    
    if RNG:NextDouble() < flipChance then
      if parent == genomeA then
        parent = genomeB
      else
        parent = genomeA
      end
    end
  end
  
  return child
end