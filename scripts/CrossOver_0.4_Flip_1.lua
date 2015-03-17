-- Does crossover with 40%, mutate with 1 bit flip

require 'DoCrossOver'
require 'FlipGenomeBits'

function CrossOver(genomeA, genomeB)
  return DoCrossOver(genomeA, genomeB, 0.4)
end

function Mutate(genome)
  return FlipGenomeBits(genome, 1)
end