-- Randomly combines two genome strings to make a child.
function CrossOver(genomeA, genomeB)  
  local child = ""
  local parent
  
  -- select the initial parent
  if RNG:NextDouble() < 0.5 then
    parent = genomeA
  else
    parent = genomeB
  end
  
  -- build the genome string
  for i = 1, GenomeLength do
    -- add the next bit from the parent
    child = child .. parent:sub(i, i)
    
    -- 40% chance to swap parents
    if RNG:NextDouble() < 0.4 then
      if parent == genomeA then
        parent = genomeB
      else
        parent = genomeA
      end
    end
  end
  
  return child
end

-- Flips a random bit in a genome string.
function Mutate(genome)
  -- select the bit to flip
  local idx = RNG:Next(GenomeLength) + 1
  local c = genome:sub(idx, idx)
  
  -- flip it (flip it good)
  if c == "0" then
    c = "1"
  else
    c = "0"
  end
  
  -- rebuild the string with the new bit
  genome = genome:sub(1, idx - 1) .. c .. genome:sub(idx + 1)
  return genome
end